// Explicit Play Mode checks using the real audio source and isolated switch collisions.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Requires Play Mode");
var audio = UnityEngine.Object.FindFirstObjectByType<Midterm.GameAudio>();
var so = new UnityEditor.SerializedObject(audio);
var effects = (UnityEngine.AudioSource)so.FindProperty("effects").objectReferenceValue;
var checkpointClip = (UnityEngine.AudioClip)so.FindProperty("checkpoint").objectReferenceValue;
var switchClip = (UnityEngine.AudioClip)so.FindProperty("switchPress").objectReferenceValue;
var results = new System.Collections.Generic.List<string>();
void Check(bool ok, string label) { if (!ok) throw new System.Exception(label); results.Add(label); }
Check(checkpointClip != null && checkpointClip.length > 0 && switchClip != null && switchClip.length > 0, "Checkpoint and switch audio clips are assigned");
var scene = UnityEngine.SceneManagement.SceneManager.CreateScene("InteractionSoundValidation", new UnityEngine.SceneManagement.CreateSceneParameters(UnityEngine.SceneManagement.LocalPhysicsMode.Physics2D));
var physics = scene.GetPhysicsScene2D();
UnityEngine.GameObject Spawn(string path, UnityEngine.Vector2 position)
{
    var go = UnityEngine.Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path), position, UnityEngine.Quaternion.identity);
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
    return go;
}
var tile = UnityEngine.ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
try
{
    var flag = Spawn("Assets/Prefabs/Object/CheckpointFlag.prefab", new UnityEngine.Vector2(20, 0)).GetComponent<Midterm.CheckpointFlag>();
    effects.Stop();
    flag.SetActivated(true);
    Check(flag.IsActivated && effects.isPlaying, "Checkpoint activation plays sound");
    effects.Stop();
    flag.SetActivated(true);
    Check(!effects.isPlaying, "Already active checkpoint does not repeat sound");
    flag.SetActivated(false);
    Check(!effects.isPlaying, "Checkpoint deactivation stays silent");
    flag.SetActivated(true);
    Check(effects.isPlaying, "Reactivating a checkpoint plays sound again");

    var grid = new UnityEngine.GameObject("Grid", typeof(UnityEngine.Grid));
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(grid, scene);
    var mapGo = new UnityEngine.GameObject("TargetTiles", typeof(UnityEngine.Tilemaps.Tilemap));
    mapGo.transform.SetParent(grid.transform, false);
    var map = mapGo.GetComponent<UnityEngine.Tilemaps.Tilemap>();
    map.SetTile(UnityEngine.Vector3Int.zero, tile);
    var button = Spawn("Assets/Prefabs/Object/FloorSwitch.prefab", UnityEngine.Vector2.zero).GetComponent<Midterm.FloorSwitch>();
    var buttonSo = new UnityEditor.SerializedObject(button);
    buttonSo.FindProperty("targetTilemap").objectReferenceValue = map;
    buttonSo.ApplyModifiedPropertiesWithoutUndo();
    var player = Spawn("Assets/Prefabs/Character/Player/Player.prefab", new UnityEngine.Vector2(0, 2)).GetComponent<Midterm.PlayerController2D>();
    var body = player.GetComponent<UnityEngine.Rigidbody2D>(); body.gravityScale = 0;
    var bounds = button.GetComponent<UnityEngine.Collider2D>().bounds;
    var position = new UnityEngine.Vector3(bounds.center.x, bounds.center.y + player.GetComponent<UnityEngine.Collider2D>().bounds.extents.y + .02f, 0);
    player.transform.position = position; body.position = position; body.linearVelocity = UnityEngine.Vector2.down;
    effects.Stop();
    UnityEngine.Physics2D.SyncTransforms();
    physics.Simulate(.02f); physics.Simulate(.02f);
    Check(button.IsPressed && !map.HasTile(UnityEngine.Vector3Int.zero) && effects.isPlaying, "Actual switch overlap plays sound and removes target tiles");
    effects.Stop();
    for (int i = 0; i < 5; i++) physics.Simulate(.02f);
    Check(!effects.isPlaying, "Standing on pressed switch does not repeat sound");
    return results.ToArray();
}
finally
{
    effects.Stop();
    foreach (var go in scene.GetRootGameObjects()) UnityEngine.Object.DestroyImmediate(go);
    UnityEngine.Object.DestroyImmediate(tile);
    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
}
