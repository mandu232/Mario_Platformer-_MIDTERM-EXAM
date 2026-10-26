// Explicit Play Mode integration checks. Async coroutine results are in SessionState.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Requires Play Mode");
const string key = "Midterm.PrefabEffectValidation";
UnityEditor.SessionState.SetString(key, "Running");
UnityEditor.EditorApplication.isPaused = true;
var scene = UnityEngine.SceneManagement.SceneManager.CreateScene("PrefabEffectValidation",
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
try
{
    var player = Spawn("Assets/Prefabs/Character/Player/Player.prefab", new UnityEngine.Vector2(0, 1)).GetComponent<Midterm.PlayerController2D>();
    var pb = player.GetComponent<UnityEngine.Rigidbody2D>(); pb.gravityScale = 0;
    var box = Spawn("Assets/Prefabs/Object/CoinItemBox.prefab", new UnityEngine.Vector2(0, 3)).GetComponent<Midterm.ItemBox>();
    pb.linearVelocity = UnityEngine.Vector2.up * 10;
    UnityEngine.Physics2D.SyncTransforms();
    for (int i = 0; i < 12; i++) physics.Simulate(.02f);
    Check(box.IsUsed, "Real head collision activates item box");
    Check(box.transform.Find("BoxBump") != null && box.transform.Find("BoxBump").GetComponent<UnityEngine.SpriteRenderer>().sprite != null, "Box bump uses configured sprite prefab");
    Check(box.GetComponentsInChildren<UnityEngine.SpriteRenderer>().Any(r => r.name.StartsWith("BoxCoin") && r.sprite != null), "Coin pop uses configured coin prefab");
    pb.simulated = false;

    var block = Spawn("Assets/Prefabs/Object/BreakableBlock.prefab", new UnityEngine.Vector2(10, 4)).GetComponent<Midterm.BreakableBlock>();
    block.SendMessage("Break");
    var effect = scene.GetRootGameObjects().Single(g => g.GetComponent<Midterm.BlockBreakEffect>() != null);
    var pieces = effect.GetComponentsInChildren<Midterm.BlockFragmentMotion>();
    Check(pieces.Length == 4 && effect.GetComponentsInChildren<UnityEngine.SpriteRenderer>().All(r => r.sprite != null), "Block effect prefab contains four authored fragments");
    foreach (var piece in pieces) piece.SendMessage("Start");
    Check(pieces.All(p => p.GetComponent<UnityEngine.Rigidbody2D>().linearVelocity.y > 0 && Mathf.Approximately(p.GetComponent<UnityEngine.Rigidbody2D>().gravityScale, 2)), "Fragment prefab launch motion and gravity are preserved");

    var boss = Spawn("Assets/Prefabs/Character/Enemies/FlyingBoss.prefab", new UnityEngine.Vector2(30, 6)).GetComponent<Midterm.FlyingBoss>();
    Check(boss.transform.Cast<UnityEngine.Transform>().Count(t => t.name == "BossVisual") == 1, "Boss uses one pre-authored visual child");
    Check(!boss.GetComponent<UnityEngine.SpriteRenderer>().enabled, "Boss root renderer stays disabled");
    typeof(Midterm.FlyingBoss).GetProperty("IsActivated").SetValue(boss, false);
    boss.ActivateEncounter(player);
    var mask = scene.GetRootGameObjects().Select(g => g.GetComponent<UnityEngine.SpriteMask>()).Single(m => m != null);
    Check(mask.sprite != null && UnityEditor.EditorUtility.IsPersistent(mask.sprite), "Boss entrance uses an imported mask sprite from the prefab");
    Check(player.IsControlLocked, "Boss entrance still locks player controls");
    for (int i = 0; i < 70; i++) boss.SendMessage("FixedUpdate");
    Check(!player.IsControlLocked && boss.State == Midterm.FlyingBoss.BossState.Flying, "Boss entrance completes and restores player controls");
    boss.enabled = false;
    boss.GetComponent<UnityEngine.Rigidbody2D>().simulated = false;
    double started = UnityEditor.EditorApplication.timeSinceStartup;
    UnityEditor.EditorApplication.CallbackFunction finish = null;
    finish = () =>
    {
        if (UnityEditor.EditorApplication.timeSinceStartup - started < 2.3) return;
        UnityEditor.EditorApplication.update -= finish;
        try
        {
            Check(box.GetComponent<UnityEngine.SpriteRenderer>().enabled && box.transform.Find("BoxBump") == null, "Box bump restores original renderer and cleans up");
            Check(!box.GetComponentsInChildren<UnityEngine.SpriteRenderer>().Any(r => r.name.StartsWith("BoxCoin")), "Coin pop completes and cleans up");
            Check(effect == null && mask == null, "Fragment effect and entrance mask clean up after use");
            UnityEditor.SessionState.SetString(key, "PASS\n" + string.Join("\n", results));
        }
        catch (System.Exception error) { UnityEditor.SessionState.SetString(key, "FAIL\n" + error + "\n" + string.Join("\n", results)); }
        finally { CleanUp(); UnityEditor.EditorApplication.isPaused = true; }
    };
    UnityEditor.EditorApplication.update += finish;
    UnityEditor.EditorApplication.isPaused = false;
    return "Waiting for prefab effect animations and cleanup; read SessionState " + key;
}
catch (System.Exception error)
{
    CleanUp();
    UnityEditor.SessionState.SetString(key, "FAIL\n" + error + "\n" + string.Join("\n", results));
    throw;
}
