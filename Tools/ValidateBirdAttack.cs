// Explicit Play Mode regression checks in an isolated physics scene.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Requires Play Mode");
var scene = UnityEngine.SceneManagement.SceneManager.CreateScene("BirdValidation", new UnityEngine.SceneManagement.CreateSceneParameters(UnityEngine.SceneManagement.LocalPhysicsMode.Physics2D));
var physics = scene.GetPhysicsScene2D();
var results = new System.Collections.Generic.List<string>();
void Check(bool ok, string label) { if (!ok) throw new System.Exception(label); results.Add(label); }
UnityEngine.GameObject Spawn(string path, UnityEngine.Vector2 position)
{
    var go = UnityEngine.Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path), position, UnityEngine.Quaternion.identity);
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
    return go;
}
try
{
    var player = Spawn("Assets/Prefabs/Character/Player/Player.prefab", new UnityEngine.Vector2(30, 0)).GetComponent<Midterm.PlayerController2D>();
    var pb = player.GetComponent<UnityEngine.Rigidbody2D>();
    pb.simulated = false;
    var bird = Spawn("Assets/Prefabs/Character/Enemies/FlyingEnemy.prefab", new UnityEngine.Vector2(0, 4)).GetComponent<Midterm.FlyingEnemy>();
    var bb = bird.GetComponent<UnityEngine.Rigidbody2D>();
    var so = new UnityEditor.SerializedObject(bird);
    so.FindProperty("player").objectReferenceValue = player;
    so.FindProperty("attackCooldown").floatValue = .1f;
    so.ApplyModifiedPropertiesWithoutUndo();
    bird.SendMessage("Start");
    for (int i = 0; i < 100; i++) { bird.SendMessage("FixedUpdate"); physics.Simulate(.02f); }
    Check(bird.State == Midterm.FlyingEnemy.AttackState.Patrolling, "Bird does not attack outside detection range");
    player.transform.position = new UnityEngine.Vector3(bb.position.x, 0, 0);
    for (int i = 0; i < 10 && bird.State != Midterm.FlyingEnemy.AttackState.Diving; i++) bird.SendMessage("FixedUpdate");
    Check(bird.State == Midterm.FlyingEnemy.AttackState.Diving, "Bird detects player below within X range");
    float initialY = bb.position.y;
    bool returned = false;
    bool dived = false;
    for (int i = 0; i < 300; i++)
    {
        bird.SendMessage("FixedUpdate"); physics.Simulate(.02f);
        dived |= bb.position.y < initialY - 1;
        returned |= bird.State == Midterm.FlyingEnemy.AttackState.Returning;
        if (returned && bird.State == Midterm.FlyingEnemy.AttackState.Patrolling) break;
    }
    Check(dived && returned && UnityEngine.Vector2.Distance(bb.position, new UnityEngine.Vector2(0, 4)) < .15f, "Bird dives and returns to patrol origin");
    UnityEngine.Object.DestroyImmediate(bird.gameObject);
    void Collision(bool powered)
    {
        player.Revive(new UnityEngine.Vector3(0, 20, 0));
        typeof(Midterm.PlayerController2D).GetField("invulnerableUntil", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(player, 0f);
        if (powered) player.GrantFireFlower();
        pb.gravityScale = 0;
        bird = Spawn("Assets/Prefabs/Character/Enemies/FlyingEnemy.prefab", new UnityEngine.Vector2(2, 20)).GetComponent<Midterm.FlyingEnemy>();
        bb = bird.GetComponent<UnityEngine.Rigidbody2D>();
        bb.linearVelocity = UnityEngine.Vector2.left * 8;
        UnityEngine.Physics2D.SyncTransforms();
        for (int i = 0; i < 20; i++) physics.Simulate(.02f);
        if (powered) Check(!player.IsPoweredUp && player.CurrentHealth == player.MaxHealth && !player.IsDead, "Bird collision removes power-up before health or life");
        else
        {
            Check(!player.IsDead && player.CurrentHealth == player.MaxHealth - 1, "Unpowered bird collision removes exactly one health point");
            Check(player.IsInvulnerable, "Bird collision grants the normal invulnerability window");
            int health = player.CurrentHealth;
            for (int i = 0; i < 10; i++) player.ReceiveEnemyHit(bird.transform.position);
            Check(player.CurrentHealth == health, "Repeated contact cannot stack damage during invulnerability");
        }
        UnityEngine.Object.DestroyImmediate(bird.gameObject);
    }
    Collision(true);
    Collision(false);
    return results.ToArray();
}
finally
{
    foreach (var go in scene.GetRootGameObjects()) UnityEngine.Object.DestroyImmediate(go);
    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
}
