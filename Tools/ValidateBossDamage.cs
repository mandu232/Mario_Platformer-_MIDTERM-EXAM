// Current boss contract: airborne fireball immunity, smaller stun collider, grounded damage shake.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Requires Play Mode");
var scene=UnityEngine.SceneManagement.SceneManager.CreateScene("BossDamageValidation",new UnityEngine.SceneManagement.CreateSceneParameters(UnityEngine.SceneManagement.LocalPhysicsMode.Physics2D));
var physics=scene.GetPhysicsScene2D();
var results=new System.Collections.Generic.List<string>();
void Check(bool ok,string label){if(!ok)throw new System.Exception(label);results.Add(label);}
UnityEngine.GameObject Spawn(string path,UnityEngine.Vector2 position)
{
    var prefab=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path);
    if(prefab==null)
    {
        string name=System.IO.Path.GetFileNameWithoutExtension(path);
        string movedPath=UnityEditor.AssetDatabase.FindAssets(name+" t:Prefab",new[]{"Assets/Prefabs"})
            .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
            .Single(p=>System.IO.Path.GetFileNameWithoutExtension(p)==name);
        prefab=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(movedPath);
    }
    var go=UnityEngine.Object.Instantiate(prefab,position,UnityEngine.Quaternion.identity);
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,scene);
    return go;
}
void Tick(Midterm.FlyingBoss boss,int count,Midterm.FireballProjectile shot=null)
{
    for(int i=0;i<count;i++)
    {
        if(!boss.IsDying)boss.SendMessage("FixedUpdate");
        if(shot!=null&&shot.gameObject.activeSelf)shot.SendMessage("FixedUpdate");
        physics.Simulate(.02f);
    }
}
try
{
    var floor=new UnityEngine.GameObject("Floor",typeof(UnityEngine.BoxCollider2D));
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(floor,scene);
    floor.transform.position=new UnityEngine.Vector3(10,-1,0);
    floor.GetComponent<UnityEngine.BoxCollider2D>().size=new UnityEngine.Vector2(100,1);
    var grid=new UnityEngine.GameObject("GateGrid",typeof(UnityEngine.Grid));
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(grid,scene);
    var gateGo=new UnityEngine.GameObject("TestGate",typeof(UnityEngine.Tilemaps.Tilemap),typeof(UnityEngine.Rigidbody2D),typeof(UnityEngine.Tilemaps.TilemapCollider2D),typeof(UnityEngine.CompositeCollider2D));
    gateGo.transform.SetParent(grid.transform,false);gateGo.transform.position=new UnityEngine.Vector3(50,50,0);
    gateGo.GetComponent<UnityEngine.Rigidbody2D>().bodyType=UnityEngine.RigidbodyType2D.Static;
    var gate=gateGo.GetComponent<UnityEngine.Tilemaps.Tilemap>();
    var gateCollider=gateGo.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
    gateCollider.compositeOperation=UnityEngine.Collider2D.CompositeOperation.Merge;
    var composite=gateGo.GetComponent<UnityEngine.CompositeCollider2D>();
    var tile=UnityEngine.ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();tile.colliderType=UnityEngine.Tilemaps.Tile.ColliderType.Grid;
    void ResetGate(){gate.SetTile(UnityEngine.Vector3Int.zero,tile);gateCollider.ProcessTilemapChanges();composite.GenerateGeometry();}
    ResetGate();
    var shakeGo=new UnityEngine.GameObject("DeathShakeCamera",typeof(Midterm.FollowCamera));
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(shakeGo,scene);
    var shakeCamera=shakeGo.GetComponent<Midterm.FollowCamera>();
    var p=Spawn("Assets/Prefabs/Player.prefab",new UnityEngine.Vector2(0,1)).GetComponent<Midterm.PlayerController2D>();
    var b=Spawn("Assets/Prefabs/Enemies/FlyingBoss.prefab",new UnityEngine.Vector2(0,5)).GetComponent<Midterm.FlyingBoss>();
    void Bind(Midterm.FlyingBoss boss){var so=new UnityEditor.SerializedObject(boss);so.FindProperty("player").objectReferenceValue=p;so.FindProperty("impactCamera").objectReferenceValue=shakeCamera;so.FindProperty("defeatBlocks").objectReferenceValue=gate;so.ApplyModifiedPropertiesWithoutUndo();boss.SendMessage("Start");}
    Bind(b);
    var hud=Spawn("Assets/Prefabs/UI/BossHealthHud.prefab",UnityEngine.Vector2.zero).GetComponent<Midterm.BossHealthHud>();
    hud.ShowEntrance(b);
    var hudHearts=hud.GetComponentsInChildren<UnityEngine.UI.Image>();
    var collider=b.GetComponent<UnityEngine.CircleCollider2D>();
    float normalRadius=collider.radius;
    Check(UnityEngine.Mathf.Approximately(normalRadius,.65f*.8f),"Airborne collider is 20 percent smaller");
    var airBall=Spawn("Assets/Prefabs/Items/Fireball.prefab",(UnityEngine.Vector2)b.transform.position+UnityEngine.Vector2.left*2.2f).GetComponent<Midterm.FireballProjectile>();
    airBall.Launch(p,1);Tick(b,5,airBall);
    Check(!airBall.gameObject.activeSelf&&b.FireballHits==0&&b.State==Midterm.FlyingBoss.BossState.Flying,"Actual airborne projectile is blocked without damage or stun");
    UnityEngine.Object.DestroyImmediate(airBall.gameObject);
    var immuneStates=new System.Collections.Generic.HashSet<Midterm.FlyingBoss.BossState>();
    bool spun=false;
    bool spunDuringCharge=false;
    for(int i=0;i<500;i++)
    {
        var oldState=b.State;var oldAngle=b.transform.Find("BossVisual").localEulerAngles.z;
        Tick(b,1);
        if(oldState==Midterm.FlyingBoss.BossState.Charging && b.State==Midterm.FlyingBoss.BossState.Charging && UnityEngine.Mathf.Abs(UnityEngine.Mathf.DeltaAngle(oldAngle,b.transform.Find("BossVisual").localEulerAngles.z))>1)spunDuringCharge=true;
        if(b.State==Midterm.FlyingBoss.BossState.Stunned)break;
        var state=b.State;
        b.ReceiveFireball(-10);
        if(b.State!=state||b.FireballHits!=0)throw new System.Exception("Airborne fireball changed boss state or health");
        immuneStates.Add(state);
        if(state==Midterm.FlyingBoss.BossState.WindingUp&&b.transform.Find("BossVisual").localEulerAngles.z>1)spun=true;
    }
    Check(immuneStates.Contains(Midterm.FlyingBoss.BossState.Flying)&&immuneStates.Contains(Midterm.FlyingBoss.BossState.WindingUp)&&immuneStates.Contains(Midterm.FlyingBoss.BossState.Charging),"Flying/windup/charge ignore fireball damage and stun");
    Check(spun,"Spin stays on visual child");
    Check(b.State==Midterm.FlyingBoss.BossState.Stunned,"Charge still lands and stuns");
    Check(collider.radius<normalRadius&&UnityEngine.Mathf.Approximately(collider.radius,normalRadius*.55f/.8f),"Stun collider shrinks to 55 percent");
    var visual=b.transform.Find("BossVisual");
    Check(spunDuringCharge,"Boss keeps spinning throughout charge");
    var impactRotation=visual.localRotation;Tick(b,4);
    Check(UnityEngine.Quaternion.Angle(impactRotation,visual.localRotation)<.001f,"Stun preserves exact final impact angle");
    b.SendMessage("LateUpdate");
    Check(visual.localPosition.y<0 && visual.localScale==UnityEngine.Vector3.one,"Landing embeds unscaled visual at impact angle");
    Check(collider.bounds.min.y>=-.52f,"Embedded visual leaves physics collider above floor");
    Check(visual.GetComponent<UnityEngine.SpriteRenderer>().sprite.name=="Boss_Stunned_0","User supplied stunned sprite displayed");
    p.Respawn(new UnityEngine.Vector3(-20,1,0));
    for(int n=1;n<=9;n++)
    {
        var ball=Spawn("Assets/Prefabs/Items/Fireball.prefab",(UnityEngine.Vector2)b.transform.position+UnityEngine.Vector2.left*1.15f).GetComponent<Midterm.FireballProjectile>();ball.Launch(p,1);
        for(int i=0;i<15&&ball.gameObject.activeSelf;i++)Tick(b,1,ball);
        Check(b.FireballHits==n&&!ball.gameObject.activeSelf,"Grounded actual projectile damage "+n);
        UnityEngine.Object.DestroyImmediate(ball.gameObject);
    }
    Check(!b.IsDying&&b.RemainingHits==1,"Nine grounded hits do not defeat boss");
    Check(hudHearts[0].sprite.name=="Boss_Heart_1"&&hudHearts[4].sprite.name=="Boss_Heart_2","Boss HUD shows half final heart after nine hits");
    Check(gate.HasTile(UnityEngine.Vector3Int.zero)&&composite.pathCount>0,"Gate remains closed before defeat");
    var fixedPosition=b.GetComponent<UnityEngine.Rigidbody2D>().position;
    b.SendMessage("LateUpdate");
    Check(visual.localPosition.sqrMagnitude>0,"Valid damage visibly shakes boss");
    Check(b.GetComponent<UnityEngine.Rigidbody2D>().position==fixedPosition,"Shake leaves physics body in place");
    for(int i=0;i<1000;i++)b.SendMessage("LateUpdate");
    Check(UnityEngine.Mathf.Abs(visual.localPosition.x)<.0001f && UnityEngine.Mathf.Abs(visual.localPosition.y+.2f)<.0001f,"Damage shake returns to embedded resting pose");
    b.ReceiveFireball(-20);
    Check(b.IsDying&&b.FireballHits==10,"Tenth grounded hit defeats boss");
    float peakShake=0;
    for(int i=0;i<8;i++){shakeCamera.SendMessage("LateUpdate");peakShake=UnityEngine.Mathf.Max(peakShake,shakeGo.transform.position.magnitude);}
    Check(peakShake>.22f,"Death triggers a stronger screen shake than landing");
    Check(hudHearts.All(h=>h.sprite.name=="Boss_Heart_2"),"Tenth hit empties all boss hearts");
    hud.SendMessage("Update");
    Check(hudHearts.All(h=>!h.enabled),"Boss death hides all heart images");
    Check(b.transform.Find("BossVisual").GetComponent<UnityEngine.SpriteRenderer>().enabled&&b.GetComponent<UnityEngine.Rigidbody2D>().simulated&&b.GetComponent<UnityEngine.Rigidbody2D>().linearVelocity.y>0&&UnityEngine.Mathf.Abs(b.GetComponent<UnityEngine.Rigidbody2D>().angularVelocity)>0,"Defeated boss stays visible and launches with spin");
    Check(!gate.HasTile(UnityEngine.Vector3Int.zero)&&composite.pathCount==0,"Fireball defeat opens gate and removes composite collision");
    UnityEngine.Object.DestroyImmediate(b.gameObject);
    p.enabled=false;p.enabled=true;p.Heal(100);p.Respawn(new UnityEngine.Vector3(20,1,0));
    b=Spawn("Assets/Prefabs/Enemies/FlyingBoss.prefab",new UnityEngine.Vector2(20,5)).GetComponent<Midterm.FlyingBoss>();Bind(b);
    for(int i=0;i<500&&b.State!=Midterm.FlyingBoss.BossState.Stunned;i++)Tick(b,1);
    Check(b.State==Midterm.FlyingBoss.BossState.Stunned,"Second boss reaches stun naturally");
    p.Respawn(new UnityEngine.Vector3(-20,1,0));
    for(int i=0;i<200&&b.State!=Midterm.FlyingBoss.BossState.Recovering;i++)Tick(b,1);
    Check(b.State==Midterm.FlyingBoss.BossState.Recovering&&UnityEngine.Mathf.Approximately(b.GetComponent<UnityEngine.CircleCollider2D>().radius,normalRadius),"Recovery restores full collider size");
    Check(b.GetComponent<UnityEngine.Collider2D>().bounds.min.y>=-.52f,"Collider expansion does not penetrate floor");
    b.ReceiveFireball(-20);
    Check(b.FireballHits==0&&b.State==Midterm.FlyingBoss.BossState.Recovering,"Recovery also ignores fireball damage and stun");
    UnityEngine.Object.DestroyImmediate(b.gameObject);
    ResetGate();
    p.enabled=false;p.enabled=true;p.Heal(100);p.Respawn(new UnityEngine.Vector3(20,1,0));
    b=Spawn("Assets/Prefabs/Enemies/FlyingBoss.prefab",new UnityEngine.Vector2(20,5)).GetComponent<Midterm.FlyingBoss>();Bind(b);
    for(int i=0;i<500&&b.State!=Midterm.FlyingBoss.BossState.Stunned;i++)Tick(b,1);
    var playerBody=p.GetComponent<UnityEngine.Rigidbody2D>();
    playerBody.position=b.GetComponent<UnityEngine.Collider2D>().bounds.center+UnityEngine.Vector3.up*(b.GetComponent<UnityEngine.Collider2D>().bounds.extents.y+p.GetComponent<UnityEngine.Collider2D>().bounds.extents.y+.15f);
    playerBody.linearVelocity=UnityEngine.Vector2.down*8;
    Tick(b,12);
    Check(!b.IsDying&&b.FireballHits==0&&playerBody.linearVelocity.y>0,"Stomping bounces player without boss damage or death");
    Check(gate.HasTile(UnityEngine.Vector3Int.zero)&&composite.pathCount>0,"Stomp leaves boss gate closed");
    UnityEngine.Object.DestroyImmediate(tile);
    return results.ToArray();
}
finally
{
    foreach(var go in scene.GetRootGameObjects())UnityEngine.Object.DestroyImmediate(go);
    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
}




