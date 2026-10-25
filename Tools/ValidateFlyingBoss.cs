// Run explicitly in Play Mode. Local physics scene is always cleaned up.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Requires Play Mode");
var scene = UnityEngine.SceneManagement.SceneManager.CreateScene("BossValidation", new UnityEngine.SceneManagement.CreateSceneParameters(UnityEngine.SceneManagement.LocalPhysicsMode.Physics2D));
var physics = scene.GetPhysicsScene2D();
var results = new System.Collections.Generic.List<string>();
void Check(bool condition, string message) { if (!condition) throw new System.Exception(message); results.Add(message); }
UnityEngine.GameObject Spawn(string path, UnityEngine.Vector2 position)
{
    var go = UnityEngine.Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path), position, UnityEngine.Quaternion.identity);
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
    return go;
}
void Tick(Midterm.FlyingBoss boss, int count, Midterm.FireballProjectile shot = null)
{
    for (int i=0;i<count;i++)
    {
        if(boss != null && !boss.IsDying) boss.SendMessage("FixedUpdate");
        if(shot != null && shot.gameObject.activeSelf) shot.SendMessage("FixedUpdate");
        physics.Simulate(.02f);
    }
}
try
{
    var floor = new UnityEngine.GameObject("BossTestFloor", typeof(UnityEngine.BoxCollider2D));
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(floor,scene);
    floor.transform.position = new UnityEngine.Vector3(20,-1,0);
    floor.GetComponent<UnityEngine.BoxCollider2D>().size = new UnityEngine.Vector2(100,1);
    var player = Spawn("Assets/Prefabs/Player.prefab",new UnityEngine.Vector2(0,1)).GetComponent<Midterm.PlayerController2D>();
    var body = player.GetComponent<UnityEngine.Rigidbody2D>();
    var boss = Spawn("Assets/Prefabs/Enemies/FlyingBoss.prefab",new UnityEngine.Vector2(0,5)).GetComponent<Midterm.FlyingBoss>();
    var so = new UnityEditor.SerializedObject(boss); so.FindProperty("player").objectReferenceValue = player; so.ApplyModifiedPropertiesWithoutUndo();
    boss.SendMessage("Start");
    var states = new System.Collections.Generic.HashSet<Midterm.FlyingBoss.BossState>();
    bool rotated = false;
    for(int i=0;i<450;i++)
    {
        Tick(boss,1); states.Add(boss.State);
        if(boss.State==Midterm.FlyingBoss.BossState.WindingUp && UnityEngine.Mathf.Abs(boss.transform.eulerAngles.z)>1) rotated=true;
        if(boss.State==Midterm.FlyingBoss.BossState.Stunned)break;
    }
    Check(states.Contains(Midterm.FlyingBoss.BossState.Flying),"Boss patrols in air");
    Check(states.Contains(Midterm.FlyingBoss.BossState.WindingUp) && rotated,"Boss spins before attacking");
    Check(states.Contains(Midterm.FlyingBoss.BossState.Charging),"Boss charges toward player after windup");
    Check(boss.State==Midterm.FlyingBoss.BossState.Stunned,"Floor impact stuns boss");
    player.Heal(100);
    body.position = boss.GetComponent<UnityEngine.Collider2D>().bounds.center + UnityEngine.Vector3.up * (boss.GetComponent<UnityEngine.Collider2D>().bounds.extents.y + player.GetComponent<UnityEngine.Collider2D>().bounds.extents.y + .15f);
    body.linearVelocity=UnityEngine.Vector2.down*8;
    Tick(boss,12);
    Check(boss.IsDying && body.linearVelocity.y>0,"Landing on stunned boss defeats it and bounces player");
    UnityEngine.Object.DestroyImmediate(boss.gameObject);
    body.position=new UnityEngine.Vector2(-20,1); body.linearVelocity=UnityEngine.Vector2.zero;
    boss=Spawn("Assets/Prefabs/Enemies/FlyingBoss.prefab",new UnityEngine.Vector2(20,5)).GetComponent<Midterm.FlyingBoss>();
    boss.SendMessage("Start");
    for(int n=1;n<=10;n++)
    {
        var ball=Spawn("Assets/Prefabs/Items/Fireball.prefab",(UnityEngine.Vector2)boss.transform.position+UnityEngine.Vector2.left*1.6f).GetComponent<Midterm.FireballProjectile>();
        ball.Launch(player,1);
        for(int frame=0;frame<30 && ball.gameObject.activeSelf;frame++)Tick(boss,1,ball);
        Check(boss.FireballHits==n && !ball.gameObject.activeSelf,"Actual fireball hit "+n+" counted exactly once");
        if(n==1)Check(boss.State==Midterm.FlyingBoss.BossState.Falling && boss.GetComponent<UnityEngine.Rigidbody2D>().gravityScale>0,"Airborne hit makes boss fall stunned");
        if(n==9)Check(!boss.IsDying && boss.RemainingHits==1,"Boss survives nine fireballs");
        UnityEngine.Object.DestroyImmediate(ball.gameObject);
    }
    Check(boss.IsDying && !boss.GetComponent<UnityEngine.Collider2D>().enabled,"Tenth fireball defeats boss");
    UnityEngine.Object.DestroyImmediate(boss.gameObject);
    boss=Spawn("Assets/Prefabs/Enemies/FlyingBoss.prefab",new UnityEngine.Vector2(40,5)).GetComponent<Midterm.FlyingBoss>();
    boss.SendMessage("Start"); boss.ReceiveFireball(0);
    for(int i=0;i<150 && boss.State!=Midterm.FlyingBoss.BossState.Stunned;i++)Tick(boss,1);
    Check(boss.State==Midterm.FlyingBoss.BossState.Stunned,"Shot boss lands in stompable state");
    Tick(boss,170);
    Check(boss.State==Midterm.FlyingBoss.BossState.Recovering && !boss.IsDying,"Unstomped boss recovers after stun");
    Tick(boss,180);
    Check(boss.State==Midterm.FlyingBoss.BossState.Flying,"Recovered boss resumes flight");
    UnityEngine.Object.DestroyImmediate(boss.gameObject);
    var wall=new UnityEngine.GameObject("BossTestWall",typeof(UnityEngine.BoxCollider2D));
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(wall,scene);
    wall.transform.position=new UnityEngine.Vector3(4,5,0);
    wall.GetComponent<UnityEngine.BoxCollider2D>().size=new UnityEngine.Vector2(.3f,15);
    player.Heal(100); body.position=new UnityEngine.Vector2(8,1);body.linearVelocity=UnityEngine.Vector2.zero;
    boss=Spawn("Assets/Prefabs/Enemies/FlyingBoss.prefab",new UnityEngine.Vector2(0,6)).GetComponent<Midterm.FlyingBoss>();
    so=new UnityEditor.SerializedObject(boss);so.FindProperty("player").objectReferenceValue=player;so.ApplyModifiedPropertiesWithoutUndo();boss.SendMessage("Start");
    bool wallStun=false;
    for(int i=0;i<600;i++)
    {
        Tick(boss,1);
        if(boss.State==Midterm.FlyingBoss.BossState.Falling && boss.transform.position.y>2 && boss.transform.position.x>1.5f){wallStun=true;break;}
    }
    Check(wallStun,"Charge hitting wall stuns boss in air");
    for(int i=0;i<150 && boss.State!=Midterm.FlyingBoss.BossState.Stunned;i++)Tick(boss,1);
    Check(boss.State==Midterm.FlyingBoss.BossState.Stunned,"Wall-stunned boss falls and becomes stompable on floor");
    UnityEngine.Object.DestroyImmediate(wall);
    UnityEngine.Object.DestroyImmediate(boss.gameObject);
    player.enabled=false; player.enabled=true;
    player.Heal(100);
    boss=Spawn("Assets/Prefabs/Enemies/FlyingBoss.prefab",new UnityEngine.Vector2(40,5)).GetComponent<Midterm.FlyingBoss>();
    boss.SendMessage("Start");
    body.position=new UnityEngine.Vector2(40,5+boss.GetComponent<UnityEngine.Collider2D>().bounds.extents.y+player.GetComponent<UnityEngine.Collider2D>().bounds.extents.y+.15f);
    body.linearVelocity=UnityEngine.Vector2.down*8;
    int health=player.CurrentHealth;
    Tick(boss,12);
    Check(!boss.IsDying && player.CurrentHealth==health-1,"Unstunned spiked boss resists stomp and damages player");
    return results.ToArray();
}
finally
{
    foreach(var go in scene.GetRootGameObjects())UnityEngine.Object.DestroyImmediate(go);
    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
}


