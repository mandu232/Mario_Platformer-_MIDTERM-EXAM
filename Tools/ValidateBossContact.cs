// Explicit Play Mode regression: real boss collisions, grace period and cleanup.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Requires Play Mode");
const string key = "Midterm.BossContactValidation";
UnityEditor.SessionState.SetString(key, "Running");
UnityEditor.EditorApplication.isPaused = true;
var scene = UnityEngine.SceneManagement.SceneManager.CreateScene("BossContactValidation",
    new UnityEngine.SceneManagement.CreateSceneParameters(UnityEngine.SceneManagement.LocalPhysicsMode.Physics2D));
var physics = scene.GetPhysicsScene2D();
var results = new System.Collections.Generic.List<string>();
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
void Check(bool ok, string label) { if (!ok) throw new System.Exception(label); results.Add(label); }
UnityEngine.GameObject Spawn(string path, UnityEngine.Vector2 position)
{
    var go = UnityEngine.Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path), position, UnityEngine.Quaternion.identity);
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
    return go;
}
void CleanUp()
{
    foreach (var go in scene.GetRootGameObjects()) UnityEngine.Object.DestroyImmediate(go);
    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
}
try
{
    Midterm.PlayerController2D lastPlayer = null;
    Midterm.FlyingBoss lastBoss = null;
    foreach (bool powered in new[] { true, false })
    foreach (var direction in new[] { UnityEngine.Vector2.right, new UnityEngine.Vector2(1, -1).normalized })
    {
        var player = Spawn("Assets/Prefabs/Character/Player/Player.prefab", new UnityEngine.Vector2(0, 10)).GetComponent<Midterm.PlayerController2D>();
        var pb = player.GetComponent<UnityEngine.Rigidbody2D>(); pb.gravityScale = 0;
        if (powered) player.GrantFireFlower();
        var boss = Spawn("Assets/Prefabs/Character/Enemies/FlyingBoss.prefab", new UnityEngine.Vector2(0, 10) - direction * 2).GetComponent<Midterm.FlyingBoss>();
        var bb = boss.GetComponent<UnityEngine.Rigidbody2D>();
        typeof(Midterm.FlyingBoss).GetProperty("State").SetValue(boss, Midterm.FlyingBoss.BossState.Charging);
        typeof(Midterm.FlyingBoss).GetField("chargeDirection", flags).SetValue(boss, direction);
        typeof(Midterm.FlyingBoss).GetField("timer", flags).SetValue(boss, 2f);
        UnityEngine.Physics2D.SyncTransforms();
        for (int i = 0; i < 30; i++) { boss.SendMessage("FixedUpdate"); physics.Simulate(.02f); }
        string label = (powered ? "Powered" : "Normal") + " " + (direction.y == 0 ? "horizontal" : "diagonal") + " impact";
        Check(!player.IsDead && player.CurrentHealth == player.MaxHealth - (powered ? 0 : 1), label + " causes no instant death");
        Check(!player.IsPoweredUp && player.IsInvulnerable, label + " applies shield loss or one health damage and immunity");
        Check(boss.State != Midterm.FlyingBoss.BossState.Charging, label + " interrupts charge");
        Check(Mathf.Abs(pb.linearVelocity.x) <= 3.1f && Mathf.Abs(pb.linearVelocity.y) <= 4.1f, label + " has reduced knockback instead of 14-unit pushing");
        var collider = player.GetComponent<UnityEngine.Collider2D>();
        Check(UnityEngine.Physics2D.GetIgnoreCollision(boss.GetComponent<UnityEngine.Collider2D>(), collider), label + " temporarily separates physical collision");
        int health = player.CurrentHealth;
        for (int i = 0; i < 10; i++) player.ReceiveBossHit(boss.transform.position, new UnityEngine.Vector2(3, 4), 2);
        Check(player.CurrentHealth == health, label + " does not stack damage during immunity");
        if (powered || direction.y == 0)
        {
            boss.enabled = false;
            Check(!UnityEngine.Physics2D.GetIgnoreCollision(boss.GetComponent<UnityEngine.Collider2D>(), collider), "Disabling boss restores collision pair");
            UnityEngine.Object.DestroyImmediate(boss.gameObject);
            UnityEngine.Object.DestroyImmediate(player.gameObject);
        }
        else { lastPlayer = player; lastBoss = boss; }
    }
    double started = UnityEditor.EditorApplication.timeSinceStartup;
    UnityEditor.EditorApplication.CallbackFunction finish = null;
    finish = () =>
    {
        if (UnityEditor.EditorApplication.timeSinceStartup - started < 2.4) return;
        UnityEditor.EditorApplication.update -= finish;
        try
        {
            Check(!UnityEngine.Physics2D.GetIgnoreCollision(lastBoss.GetComponent<UnityEngine.Collider2D>(), lastPlayer.GetComponent<UnityEngine.Collider2D>()), "Collision automatically restores after two-second grace");
            Check(!lastPlayer.IsInvulnerable && !lastPlayer.IsDead && lastPlayer.CurrentHealth == lastPlayer.MaxHealth - 1, "Player survives and temporary immunity expires normally");
            UnityEditor.SessionState.SetString(key, "PASS\n" + string.Join("\n", results));
        }
        catch (System.Exception e) { UnityEditor.SessionState.SetString(key, "FAIL\n" + e + "\n" + string.Join("\n", results)); }
        finally { CleanUp(); UnityEditor.EditorApplication.isPaused = true; }
    };
    UnityEditor.EditorApplication.update += finish;
    UnityEditor.EditorApplication.isPaused = false;
    return "Waiting for collision grace expiry; read SessionState " + key;
}
catch (System.Exception e)
{
    CleanUp();
    UnityEditor.SessionState.SetString(key, "FAIL\n" + e + "\n" + string.Join("\n", results));
    throw;
}
