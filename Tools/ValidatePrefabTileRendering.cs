// Run explicitly in Play Mode. Includes a cached editor-sprite reproduction.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Requires Play Mode");
var results = new System.Collections.Generic.List<string>();
int cells = 0, enemies = 0;
foreach (var map in UnityEngine.Object.FindObjectsByType<UnityEngine.Tilemaps.Tilemap>(UnityEngine.FindObjectsSortMode.None))
foreach (var cell in map.cellBounds.allPositionsWithin)
{
    if (!(map.GetTile(cell) is Midterm.PrefabTile)) continue;
    cells++;
    if (map.GetTile(cell) is Midterm.EnemyTile) enemies++;
    if (map.GetSprite(cell) != null && map.GetColor(cell).a > .001f)
        throw new System.Exception("Duplicate tile preview remains: " + map.name + " " + cell);
    if (map.GetInstantiatedObject(cell) == null)
        throw new System.Exception("Tile prefab instance missing: " + map.name + " " + cell);
}
results.Add("Scene " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + ": " + cells + " prefab tiles (" + enemies + " enemies), no visible tile previews or missing instances");
var scene = UnityEngine.SceneManagement.SceneManager.CreateScene("CachedTileRenderingValidation",
    new UnityEngine.SceneManagement.CreateSceneParameters(UnityEngine.SceneManagement.LocalPhysicsMode.Physics2D));
var cachedTiles = new System.Collections.Generic.List<UnityEngine.Tilemaps.Tile>();
try
{
    var grid = new UnityEngine.GameObject("Grid", typeof(UnityEngine.Grid));
    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(grid, scene);
    var mapGo = new UnityEngine.GameObject("CachedPreviewMap", typeof(UnityEngine.Tilemaps.Tilemap), typeof(UnityEngine.Tilemaps.TilemapRenderer));
    mapGo.transform.SetParent(grid.transform, false);
    var map = mapGo.GetComponent<UnityEngine.Tilemaps.Tilemap>();
    var wrapper = (UnityEngine.Tilemaps.ITilemap)System.Activator.CreateInstance(typeof(UnityEngine.Tilemaps.ITilemap),
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
        null, new object[] { map }, null);
    int index = 0;
    foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:Tile", new[] { "Assets/Sprite/Tilemap/Tiles" }))
    {
        var tile = UnityEditor.AssetDatabase.LoadAssetAtPath<Midterm.PrefabTile>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
        if (tile == null) continue;
        if (tile.gameObject == null || tile.sprite == null) throw new System.Exception("Tile asset missing prefab or preview: " + tile.name);
        var cell = new UnityEngine.Vector3Int(index++ * 4, 0, 0);
        // Mimic a Player's cached scene data without invoking the runtime TileData path.
        var cached = UnityEngine.ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
        cached.sprite = tile.sprite;
        cached.flags = UnityEngine.Tilemaps.TileFlags.LockColor;
        cached.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.None;
        cachedTiles.Add(cached);
        map.SetTile(cell, cached);
        if (map.GetSprite(cell) == null || map.GetColor(cell).a < .99f) throw new System.Exception("Invalid cache reproduction");
        var instance = UnityEngine.Object.Instantiate(tile.gameObject);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(instance, scene);
        tile.StartUp(cell, wrapper, instance);
        if (map.GetColor(cell).a > .001f) throw new System.Exception("Cached sprite not hidden: " + tile.name);
        if (!instance.GetComponentsInChildren<UnityEngine.SpriteRenderer>(true).Any(r => r.enabled && r.sprite != null && r.color.a > .001f))
            throw new System.Exception("Prefab graphic hidden: " + tile.name);
    }
    if (index == 0) throw new System.Exception("No prefab tile assets tested");
    results.Add(index + " tile assets hide opaque, color-locked cached sprites at startup while preserving prefab graphics");
    return results.ToArray();
}
finally
{
    foreach (var go in scene.GetRootGameObjects()) UnityEngine.Object.DestroyImmediate(go);
    foreach (var tile in cachedTiles) UnityEngine.Object.DestroyImmediate(tile);
    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
}
