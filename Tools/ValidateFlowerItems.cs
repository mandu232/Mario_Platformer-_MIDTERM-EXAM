// Explicit Play Mode integration checks in a disposable local 2D physics scene.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Requires Play Mode.");
var scene = UnityEngine.SceneManagement.SceneManager.CreateScene("FlowerValidation", new UnityEngine.SceneManagement.CreateSceneParameters(UnityEngine.SceneManagement.LocalPhysicsMode.Physics2D));
var physics = scene.GetPhysicsScene2D();
var results = new System.Collections.Generic.List<string>();
void Require(bool condition, string message) { if (!condition) throw new System.Exception(message); results.Add(message); }
UnityEngine.GameObject Spawn(string path, UnityEngine.Vector2 position)
{
    var go = UnityEngine.Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path), position, UnityEngine.Quaternion.identity);
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
    return go;
}
void Step(int frames, Midterm.FireballProjectile shot = null)
{
    for (int i = 0; i < frames; i++)
    {
        if (shot != null && shot.gameObject.activeSelf) shot.SendMessage("FixedUpdate");
        physics.Simulate(.02f);
    }
}
try
{
    var playerGo = Spawn("Assets/Prefabs/Player.prefab", new UnityEngine.Vector2(-20, 1));
    var player = playerGo.GetComponent<Midterm.PlayerController2D>();
    var playerBody = playerGo.GetComponent<UnityEngine.Rigidbody2D>();
    playerBody.gravityScale = 0;
    var attack = playerGo.GetComponent<Midterm.PlayerFireAttack>();
    Require(!attack.TryShoot(), "Cannot shoot before collecting FireFlower");
    player.TakeDamage(2);
    int before = player.CurrentHealth;
    var heart = Spawn("Assets/Prefabs/Items/HeartFlower.prefab", playerGo.transform.position);
    Step(2);
    Require(player.CurrentHealth == before + 1, "Heart overlap heals exactly 1");
    Step(4);
    Require(player.CurrentHealth == before + 1, "Heart cannot collect twice");
    var fire = Spawn("Assets/Prefabs/Items/FireFlower.prefab", playerGo.transform.position);
    Step(2);
    Require(attack.HasFireFlower, "FireFlower overlap grants attack");
    Require(attack.TryShoot() && !attack.TryShoot(), "Powered shot succeeds and cooldown rejects duplicate");
    foreach (var stray in UnityEngine.Object.FindObjectsByType<Midterm.FireballProjectile>(UnityEngine.FindObjectsSortMode.None))
        if (stray.gameObject.scene != scene) UnityEngine.Object.DestroyImmediate(stray.gameObject);
    var floor = new UnityEngine.GameObject("TestFloor", typeof(UnityEngine.BoxCollider2D));
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(floor, scene);
    floor.transform.position = new UnityEngine.Vector3(20, -1, 0);
    floor.GetComponent<UnityEngine.BoxCollider2D>().size = new UnityEngine.Vector2(100,1);
    var shot = Spawn("Assets/Prefabs/Items/Fireball.prefab", new UnityEngine.Vector2(0,1)).GetComponent<Midterm.FireballProjectile>();
    shot.Launch(player, 1);
    int maxBounces = 0;
    for (int i=0; i<250 && shot.gameObject.activeSelf; i++) { Step(1,shot); maxBounces = System.Math.Max(maxBounces,shot.BounceCount); }
    Require(maxBounces == 2 && !shot.gameObject.activeSelf, "Projectile bounces exactly twice and expires on third landing");
    UnityEngine.Object.DestroyImmediate(shot.gameObject);
    foreach (string name in new[]{"Small_BlueEnemy","LargeBlueEnemy","SpikedEnemy","FlyingEnemy","DroppingEnemy"})
    {
        var enemy = Spawn("Assets/Prefabs/Enemies/"+name+".prefab", new UnityEngine.Vector2(1.2f,2));
        var enemyBody = enemy.GetComponent<UnityEngine.Rigidbody2D>();
        enemyBody.gravityScale=0;
        var ball = Spawn("Assets/Prefabs/Items/Fireball.prefab", new UnityEngine.Vector2(0,2)).GetComponent<Midterm.FireballProjectile>();
        ball.Launch(player,1);
        Step(25,ball);
        bool dead = enemy.GetComponent<Midterm.StompEnemy>() != null ? enemy.GetComponent<Midterm.StompEnemy>().IsDying :
            enemy.GetComponent<Midterm.FlyingEnemy>() != null ? enemy.GetComponent<Midterm.FlyingEnemy>().IsDying : enemy.GetComponent<Midterm.DroppingEnemy>().IsDying;
        Require(dead && !ball.gameObject.activeSelf && !enemy.GetComponent<UnityEngine.Collider2D>().enabled, "One projectile defeats "+name);
        UnityEngine.Object.DestroyImmediate(enemy);
        UnityEngine.Object.DestroyImmediate(ball.gameObject);
    }
    var wall = new UnityEngine.GameObject("TestWall",typeof(UnityEngine.BoxCollider2D));
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(wall,scene);
    wall.transform.position=new UnityEngine.Vector3(2,2,0);
    wall.GetComponent<UnityEngine.BoxCollider2D>().size=new UnityEngine.Vector2(.2f,5);
    var wallShot=Spawn("Assets/Prefabs/Items/Fireball.prefab", new UnityEngine.Vector2(0,2)).GetComponent<Midterm.FireballProjectile>();
    wallShot.Launch(player,1);
    Step(25,wallShot);
    Require(!wallShot.gameObject.activeSelf && wallShot.BounceCount==0,"Wall stops projectile without tunnelling");
    UnityEngine.Object.DestroyImmediate(wall);
    var box=Spawn("Assets/Prefabs/Items/HeartFlowerBox.prefab",new UnityEngine.Vector2(-20,4)).GetComponent<Midterm.ItemBox>();
    playerBody.position=new UnityEngine.Vector2(-20,2.6f);
    playerBody.linearVelocity=new UnityEngine.Vector2(0,10);
    Step(8);
    Require(box.IsUsed,"Actual upward player collision activates box");
    Require(box.transform.Find("BoxBump")!=null,"Box bump visual starts");
    int released=UnityEngine.Object.FindObjectsByType<Midterm.FlowerPickup>(UnityEngine.FindObjectsSortMode.None).Count(f=>f.name.Contains("Heart") && f.GetComponent<UnityEngine.Collider2D>().enabled==false && f.GetComponent<UnityEngine.SpriteRenderer>().enabled);
    Require(released==1,"Head hit releases one emerging item with collection disabled");
    playerBody.position=new UnityEngine.Vector2(-20,2.6f); playerBody.linearVelocity=new UnityEngine.Vector2(0,10); Step(8);
    Require(box.IsUsed,"Used box stays latched");
    return results.ToArray();
}
finally
{
    foreach(var go in scene.GetRootGameObjects()) UnityEngine.Object.DestroyImmediate(go);
    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
}

