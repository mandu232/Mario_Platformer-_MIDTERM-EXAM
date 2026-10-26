// Run explicitly in Stage2 Play Mode. Stop Play Mode afterward to restore the scene.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Requires Play Mode");
UnityEditor.EditorApplication.isPaused = true;
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.path != "Assets/Scenes/Stage2.unity") throw new System.Exception("Requires Stage2");
var manager = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Midterm.GameManager>()).Single();
var boss = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Midterm.FlyingBoss>()).Single();
var audio = UnityEngine.Object.FindFirstObjectByType<Midterm.GameAudio>();
var message = manager.StageMessage;
var audioSo = new UnityEditor.SerializedObject(audio);
var jingles = (UnityEngine.AudioSource)audioSo.FindProperty("jingles").objectReferenceValue;
var finalClear = (UnityEngine.AudioClip)audioSo.FindProperty("finalClear").objectReferenceValue;
var results = new System.Collections.Generic.List<string>();
void Check(bool ok, string label) { if (!ok) throw new System.Exception(label); results.Add(label); }
var floor = new UnityEngine.GameObject("BossClearTestFloor", typeof(UnityEngine.BoxCollider2D));
UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(floor, scene);
try
{
    Check(!manager.IsStageCleared && !message.isActiveAndEnabled, "Clear UI hidden before boss defeat");
    Check(finalClear != null && jingles != null, "Final clear music and audio source assigned");
    boss.SendMessage("FallStunned");
    var body = boss.GetComponent<UnityEngine.Rigidbody2D>();
    var hitbox = boss.GetComponent<UnityEngine.CircleCollider2D>();
    boss.transform.position = new UnityEngine.Vector3(10000, 10000, 0);
    body.position = boss.transform.position;
    floor.transform.position = new UnityEngine.Vector3(10000, 10000 - hitbox.radius * Mathf.Abs(boss.transform.lossyScale.x) - .51f, 0);
    floor.GetComponent<UnityEngine.BoxCollider2D>().size = new UnityEngine.Vector2(10, 1);
    UnityEngine.Physics2D.SyncTransforms();
    boss.SendMessage("LandStunned");
    for (int i = 1; i < boss.MaxFireballHits; i++) boss.ReceiveFireball(9990);
    Check(boss.RemainingHits == 1 && !boss.IsDying && !manager.IsStageCleared, "Nine hits do not clear stage");
    boss.ReceiveFireball(9990);
    Check(boss.IsDying && boss.RemainingHits == 0, "Final hit defeats boss");
    Check(manager.IsStageCleared, "Boss defeat completes Stage2 immediately");
    Check(message.isActiveAndEnabled && message.text == "ALL STAGES CLEAR!", "ALL STAGES CLEAR message visible");
    Check(message.canvas != null && message.canvas.isActiveAndEnabled && message.color.a > 0 && message.rectTransform.rect.width > 0 && message.rectTransform.rect.height > 0, "Clear message has visible canvas, opacity and dimensions");
    Check(jingles.clip == finalClear && jingles.isPlaying && !jingles.mute && jingles.volume > 0, "Final clear music is playing: " + finalClear.name);
    Check(!manager.Player.enabled && !manager.Player.GetComponent<UnityEngine.Rigidbody2D>().simulated, "Player stops safely during clear screen");
    boss.ReceiveFireball(9990);
    Check(boss.FireballHits == boss.MaxFireballHits && manager.IsStageCleared && jingles.clip == finalClear, "Repeated hit does not repeat defeat or alter clear state");
    return string.Join("\n", results);
}
finally { UnityEngine.Object.DestroyImmediate(floor); }
