// Read-only regression check for nested prefab and scene layout inheritance.
var sessionAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/UI/SessionHud.prefab");
var hudAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/UI/GameHud.prefab");
var resultAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/UI/ResultScreen.prefab");
var bossAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/UI/BossHealthHud.prefab");
var results = new System.Collections.Generic.List<string>();
void Rect(UnityEngine.Transform actual, UnityEngine.Transform expected, string label)
{
    var a = (UnityEngine.RectTransform)actual;
    var e = (UnityEngine.RectTransform)expected;
    bool same = UnityEngine.Vector2.Distance(a.anchorMin, e.anchorMin) < .001f &&
        UnityEngine.Vector2.Distance(a.anchorMax, e.anchorMax) < .001f &&
        UnityEngine.Vector2.Distance(a.pivot, e.pivot) < .001f &&
        UnityEngine.Vector2.Distance(a.anchoredPosition, e.anchoredPosition) < .001f &&
        UnityEngine.Vector2.Distance(a.sizeDelta, e.sizeDelta) < .001f;
    if (!same) throw new System.Exception(label + " differs from prefab: position=" + a.anchoredPosition + " expected=" + e.anchoredPosition);
    var text = actual.GetComponent<UnityEngine.UI.Text>();
    if (text != null)
    {
        var source = expected.GetComponent<UnityEngine.UI.Text>();
        if (text.font != source.font || text.fontSize != source.fontSize || text.alignment != source.alignment)
            throw new System.Exception(label + " text styling differs from prefab");
    }
}
void Hud(Midterm.GameHud hud, string label)
{
    var session = hud.GetComponentInChildren<Midterm.SessionHudView>(true).transform;
    Rect(session, sessionAsset.transform, label + " session root");
    foreach (string name in new[] { "Lives", "Score", "Timer" }) Rect(session.Find(name), sessionAsset.transform.Find(name), label + " " + name);
    foreach (string name in new[] { "Health", "Coins", "HealthCaption", "HUDBackplate" }) Rect(hud.transform.Find(name), hudAsset.transform.Find(name), label + " " + name);
    var result = hud.GetComponentInChildren<Midterm.ResultScreenView>(true).transform;
    Rect(result, resultAsset.transform, label + " result root");
    foreach (string name in new[] { "Title", "ResultDetails", "RetryHint" }) Rect(result.Find(name), resultAsset.transform.Find(name), label + " " + name);
    results.Add(label + ": geometry and fonts match source prefabs");
}
Hud(hudAsset.GetComponent<Midterm.GameHud>(), "GameHud nested assets");
if (UnityEditor.EditorApplication.isPlaying)
{
    foreach (var hud in UnityEngine.Object.FindObjectsByType<Midterm.GameHud>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)) Hud(hud, hud.gameObject.scene.name + " runtime");
    foreach (var boss in UnityEngine.Object.FindObjectsByType<Midterm.BossHealthHud>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)) Rect(boss.transform, bossAsset.transform, boss.gameObject.scene.name + " runtime boss HUD");
}
else
{
    foreach (string path in new[] { "Assets/Scenes/Stage1.unity", "Assets/Scenes/Stage2.unity" })
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
        bool opened = !scene.IsValid() || !scene.isLoaded;
        if (opened) scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Additive);
        try
        {
            foreach (var hud in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Midterm.GameHud>(true))) Hud(hud, scene.name);
            foreach (var boss in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Midterm.BossHealthHud>(true))) Rect(boss.transform, bossAsset.transform, scene.name + " boss HUD");
        }
        finally { if (opened) UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true); }
    }
}
return results.ToArray();
