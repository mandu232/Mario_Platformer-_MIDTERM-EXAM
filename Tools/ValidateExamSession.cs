// Explicit Stage2 Play Mode test. Asynchronous results are stored in SessionState.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Requires Play Mode");
const string key = "Midterm.ExamSessionValidation";
UnityEditor.SessionState.SetString(key, "Running");
UnityEditor.EditorApplication.isPaused = true;
var manager = UnityEngine.Object.FindFirstObjectByType<Midterm.GameManager>();
var player = manager.Player;
var results = new System.Collections.Generic.List<string>();
var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
void Check(bool ok, string label) { if (!ok) throw new System.Exception(label); results.Add(label); }
void SetTime(float value) => typeof(Midterm.GameManager).GetProperty("TimeRemaining").SetValue(manager, value);
void Advance(float value) => typeof(Midterm.GameManager).GetMethod("AdvanceTimer", flags).Invoke(manager, new object[] { value });
var gmSo = new UnityEditor.SerializedObject(manager);
var message = manager.StageMessage;
var checkpoint = UnityEngine.Object.FindObjectsByType<Midterm.CheckpointFlag>(UnityEngine.FindObjectsSortMode.None).FirstOrDefault(c => c.gameObject.scene == manager.gameObject.scene);
if (checkpoint == null)
{
    var go = UnityEngine.Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/Object/CheckpointFlag.prefab"), player.transform.position + UnityEngine.Vector3.right * 2, UnityEngine.Quaternion.identity);
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, manager.gameObject.scene);
    checkpoint = go.GetComponent<Midterm.CheckpointFlag>();
}
gmSo.FindProperty("deathRestartDelay").floatValue = .05f;
gmSo.ApplyModifiedPropertiesWithoutUndo();
var spawn = gmSo.FindProperty("spawnPoint").objectReferenceValue as UnityEngine.Transform;
var initial = spawn != null ? spawn.position : player.transform.position;
void Fall()
{
    var position = new UnityEngine.Vector3(initial.x, gmSo.FindProperty("fallY").floatValue - 1, initial.z);
    player.transform.position = position;
    player.GetComponent<UnityEngine.Rigidbody2D>().position = position;
    manager.SendMessage("FixedUpdate");
}
try
{
    Check(manager.Lives == 3, "Run starts with three lives");
    SetTime(180); Advance(.5f); Advance(.5f);
    Check(Mathf.Approximately(manager.TimeRemaining, 179), "Timer counts down from 180 by elapsed seconds");
    manager.CollectCoin();
    Check(manager.Coins == 1 && manager.Score == 100, "Coin increments coin HUD and awards 100 score");
    var texts = UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Text>(UnityEngine.FindObjectsSortMode.None);
    Check(texts.Any(t => t.text == "LIVES 3") && texts.Any(t => t.text == "SCORE 100") && texts.Any(t => t.text == "TIME 179"), "HUD displays lives, score and remaining time");
    player.Revive(initial);
    player.GrantFireFlower();
    Check(player.IsInvulnerable && player.IsPoweredUp, "Fall test begins powered up with temporary immunity");
    Fall();
    Check(player.CurrentHealth == 0 && !player.IsPoweredUp, "Falling removes all health even when powered or invulnerable");
    Check(player.IsDead && manager.Lives == 2 && manager.IsRestarting, "Death consumes one life and begins respawn");
    for (int i = 0; i < 5; i++) manager.SendMessage("FixedUpdate");
    Check(manager.Lives == 2, "Remaining below fall threshold does not consume extra lives during respawn");
}
catch (System.Exception e) { UnityEditor.SessionState.SetString(key, "FAIL\n" + e); throw; }
int phase = 0;
double started = UnityEditor.EditorApplication.timeSinceStartup;
UnityEditor.EditorApplication.CallbackFunction poll = null;
poll = () =>
{
    try
    {
        if (UnityEditor.EditorApplication.timeSinceStartup - started > 8) throw new System.Exception("Respawn test timed out");
        if (manager.IsRestarting) return;
        if (phase == 0)
        {
            Check(!player.IsDead && player.CurrentHealth == player.MaxHealth && Mathf.Abs(player.transform.position.x - initial.x) < .1f, "Death without checkpoint revives at starting spawn with full health");
            Check(manager.TimeRemaining > 179 && manager.Score == 100 && manager.Coins == 1, "Respawn resets timer and retains score and coins");
            manager.ActivateCheckpoint(checkpoint);
            Check(checkpoint.IsActivated, "Checkpoint flag activates");
            Fall();
            Check(manager.Lives == 1, "Second death consumes exactly one life");
            phase = 1;
        }
        else if (phase == 1)
        {
            Check(!player.IsDead && checkpoint.IsActivated && Mathf.Abs(player.transform.position.x - checkpoint.SpawnPoint.position.x) < .1f, "Actual death revives at retained checkpoint");
            Fall();
            Check(manager.Lives == 0 && manager.IsGameOver, "Last life triggers Game Over");
            Check(message.text == "GAME OVER" && message.isActiveAndEnabled && message.transform.parent.gameObject.activeSelf, "Game Over screen is visible");
            Check(!player.enabled && !player.GetComponent<UnityEngine.Rigidbody2D>().simulated, "Game Over stops player controls and physics");
            Check(UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Text>(UnityEngine.FindObjectsSortMode.None).Any(t => t.text == "PRESS R TO RETRY"), "Game Over displays retry control");
            manager.CompleteStage("");
            Check(!manager.IsStageCleared, "Game Over cannot turn into stage clear");
            UnityEditor.SessionState.SetString(key, "PASS\n" + string.Join("\n", results));
            UnityEditor.EditorApplication.update -= poll;
            UnityEditor.EditorApplication.isPaused = true;
        }
    }
    catch (System.Exception e)
    {
        UnityEditor.SessionState.SetString(key, "FAIL\n" + e + "\n" + string.Join("\n", results));
        UnityEditor.EditorApplication.update -= poll;
        UnityEditor.EditorApplication.isPaused = true;
    }
};
UnityEditor.EditorApplication.update += poll;
UnityEditor.EditorApplication.isPaused = false;
return "Waiting for real death/respawn coroutines; read SessionState " + key;
