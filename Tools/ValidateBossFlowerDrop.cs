// Run explicitly with Unity script eval in Play Mode. Results are stored in SessionState.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Requires Play Mode");
const string resultKey = "Midterm.BossFlowerValidation";
UnityEditor.SessionState.SetString(resultKey, "Running");
UnityEditor.EditorApplication.isPaused = true;
var scene = UnityEngine.SceneManagement.SceneManager.CreateScene("BossFlowerValidation",
    new UnityEngine.SceneManagement.CreateSceneParameters(UnityEngine.SceneManagement.LocalPhysicsMode.Physics2D));
var physics = scene.GetPhysicsScene2D();
var results = new System.Collections.Generic.List<string>();
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
int Flowers() => scene.GetRootGameObjects().Count(g => g.GetComponent<Midterm.FlowerPickup>() != null);
try
{
    var floor = new UnityEngine.GameObject("Floor", typeof(UnityEngine.BoxCollider2D));
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(floor, scene);
    floor.transform.position = new UnityEngine.Vector3(0, -1, 0);
    floor.GetComponent<UnityEngine.BoxCollider2D>().size = new UnityEngine.Vector2(100, 1);
    var wall = new UnityEngine.GameObject("Wall", typeof(UnityEngine.BoxCollider2D));
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(wall, scene);
    wall.transform.position = new UnityEngine.Vector3(4, 5, 0);
    wall.GetComponent<UnityEngine.BoxCollider2D>().size = new UnityEngine.Vector2(.3f, 15);
    var player = Spawn("Assets/Prefabs/Character/Player/Player.prefab", new UnityEngine.Vector2(8, 1)).GetComponent<Midterm.PlayerController2D>();
    var playerBody = player.GetComponent<UnityEngine.Rigidbody2D>();
    var boss = Spawn("Assets/Prefabs/Character/Enemies/FlyingBoss.prefab", new UnityEngine.Vector2(0, 6)).GetComponent<Midterm.FlyingBoss>();
    var bossBody = boss.GetComponent<UnityEngine.Rigidbody2D>();
    var so = new UnityEditor.SerializedObject(boss);
    so.FindProperty("player").objectReferenceValue = player;
    so.ApplyModifiedPropertiesWithoutUndo();
    boss.SendMessage("Start");
    bool wallImpact = false;
    for (int i = 0; i < 650; i++)
    {
        boss.SendMessage("FixedUpdate");
        physics.Simulate(.02f);
        if (boss.State == Midterm.FlyingBoss.BossState.Falling && bossBody.position.y > 2 && bossBody.position.x > 1.5f) wallImpact = true;
        if (boss.State == Midterm.FlyingBoss.BossState.Stunned) break;
    }
    Check(wallImpact && boss.State == Midterm.FlyingBoss.BossState.Stunned, "Actual wall impact makes boss fall and land stunned");
    Check(Flowers() == 0, "Wall impact alone does not drop a flower");
    UnityEngine.Object.DestroyImmediate(wall);
    void Stomp()
    {
        playerBody.position = bossBody.position + UnityEngine.Vector2.up *
            (boss.GetComponent<UnityEngine.Collider2D>().bounds.extents.y + player.GetComponent<UnityEngine.Collider2D>().bounds.extents.y + .15f);
        playerBody.linearVelocity = UnityEngine.Vector2.down * 8;
        UnityEngine.Physics2D.SyncTransforms();
        for (int i = 0; i < 10; i++) physics.Simulate(.02f);
    }
    Stomp();
    Check(Flowers() == 1 && playerBody.linearVelocity.y > 0, "Stomp bounces player and drops exactly one flower");
    Check(!boss.IsDying && boss.FireballHits == 0, "Stomp preserves boss health and existing fireball combat");
    var first = scene.GetRootGameObjects().Single(g => g.GetComponent<Midterm.FlowerPickup>() != null);
    Check(!first.GetComponent<UnityEngine.Collider2D>().enabled, "Flower cannot collect during emergence");
    Stomp();
    Check(Flowers() == 1, "Repeated stomp in same stun does not duplicate flower");
    UnityEngine.Object.DestroyImmediate(first);
    playerBody.position = new UnityEngine.Vector2(-15, 1);
    boss.SendMessage("BeginRecovery");
    boss.SendMessage("FallStunned");
    boss.SendMessage("LandStunned");
    Stomp();
    Check(Flowers() == 1, "Next stun permits another flower");
    var flower = scene.GetRootGameObjects().Single(g => g.GetComponent<Midterm.FlowerPickup>() != null);
    playerBody.position = new UnityEngine.Vector2(-15, 3);
    playerBody.linearVelocity = UnityEngine.Vector2.zero;
    playerBody.simulated = false;
    bossBody.simulated = false;
    boss.enabled = false;
    var normalSize = player.GetComponent<UnityEngine.SpriteRenderer>().sprite.bounds.size;
    double started = UnityEditor.EditorApplication.timeSinceStartup;
    UnityEditor.EditorApplication.CallbackFunction finish = null;
    finish = () =>
    {
        if (UnityEditor.EditorApplication.timeSinceStartup - started < .8) return;
        UnityEditor.EditorApplication.update -= finish;
        try
        {
            Check(flower.GetComponent<UnityEngine.Collider2D>().enabled, "Emergence completes and enables collection");
            playerBody.simulated = true;
            player.transform.position = flower.transform.position;
            playerBody.position = flower.transform.position;
            playerBody.gravityScale = 0;
            UnityEngine.Physics2D.SyncTransforms();
            physics.Simulate(.02f);
            physics.Simulate(.02f);
            Check(player.IsPoweredUp, "Dropped flower grants power-up through real trigger overlap (health=" + player.CurrentHealth + ", player=" + playerBody.position + ", flower=" + flower.transform.position + ")");
            player.SendMessage("LateUpdate");
            var size = player.GetComponent<UnityEngine.SpriteRenderer>().sprite.bounds.size;
            Check(Mathf.Approximately(size.x / normalSize.x, 1.2f) && Mathf.Approximately(size.y / normalSize.y, 1.2f), "Dropped flower increases both dimensions by 20 percent");
            var before = UnityEngine.Object.FindObjectsByType<Midterm.FireballProjectile>(UnityEngine.FindObjectsSortMode.None);
            Check(player.GetComponent<Midterm.PlayerFireAttack>().TryShoot(), "Dropped flower enables fireball attack");
            foreach (var shot in UnityEngine.Object.FindObjectsByType<Midterm.FireballProjectile>(UnityEngine.FindObjectsSortMode.None))
                if (!before.Contains(shot)) UnityEngine.Object.DestroyImmediate(shot.gameObject);
            UnityEditor.SessionState.SetString(resultKey, "PASS\n" + string.Join("\n", results));
        }
        catch (System.Exception error) { UnityEditor.SessionState.SetString(resultKey, "FAIL\n" + error + "\n" + string.Join("\n", results)); }
        finally { CleanUp(); UnityEditor.EditorApplication.isPaused = true; }
    };
    UnityEditor.EditorApplication.update += finish;
    UnityEditor.EditorApplication.isPaused = false;
    return "Physics checks passed; waiting for real emergence coroutine. Read SessionState: " + resultKey;
}
catch (System.Exception error)
{
    CleanUp();
    UnityEditor.SessionState.SetString(resultKey, "FAIL\n" + error + "\n" + string.Join("\n", results));
    throw;
}
