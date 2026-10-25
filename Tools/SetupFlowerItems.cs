// Explicit Editor authoring recipe; run with Unity eval, never automatically.
if (UnityEditor.EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode first.");
const string root = "Assets/Prefabs/Items";
System.IO.Directory.CreateDirectory(root);
System.IO.Directory.CreateDirectory("Assets/Tiles/Items");
System.IO.Directory.CreateDirectory("Assets/Palettes");
UnityEditor.AssetDatabase.Refresh();
void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
{
    var so = new UnityEditor.SerializedObject(target);
    so.FindProperty(field).objectReferenceValue = value;
    so.ApplyModifiedPropertiesWithoutUndo();
}
UnityEngine.Sprite Sprite(string path, string name = null) => UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path).OfType<UnityEngine.Sprite>().First(s => name == null || s.name == name);
foreach (var path in new[]{"Assets/Sprite/Item/FireFlower.png", "Assets/Sprite/Item/HeartFlower.png", "Assets/Sprite/Effect/FireBall.png"})
{
    var importer = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);
    importer.filterMode = UnityEngine.FilterMode.Point;
    importer.mipmapEnabled = false;
    importer.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
    importer.spritePixelsPerUnit = 18;
    var platform = importer.GetDefaultPlatformTextureSettings();
    platform.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
    importer.SetPlatformTextureSettings(platform);
    importer.SaveAndReimport();
}
var golden = Sprite("Assets/Sprite/Tilemap/tilemap.png", "tilemap_10");
var used = Sprite("Assets/Sprite/Tilemap/tilemap.png", "tilemap_9");
var shotObject = new UnityEngine.GameObject("Fireball", typeof(UnityEngine.SpriteRenderer), typeof(UnityEngine.Rigidbody2D), typeof(UnityEngine.CircleCollider2D), typeof(Midterm.FireballProjectile));
shotObject.GetComponent<UnityEngine.SpriteRenderer>().sprite = Sprite("Assets/Sprite/Effect/FireBall.png");
shotObject.GetComponent<UnityEngine.SpriteRenderer>().sortingOrder = 15;
shotObject.GetComponent<UnityEngine.CircleCollider2D>().radius = .22f;
shotObject.GetComponent<UnityEngine.CircleCollider2D>().isTrigger = true;
shotObject.GetComponent<UnityEngine.Rigidbody2D>().bodyType = UnityEngine.RigidbodyType2D.Kinematic;
shotObject.GetComponent<UnityEngine.Rigidbody2D>().gravityScale = 0;
var shotPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(shotObject, root + "/Fireball.prefab");
UnityEngine.Object.DestroyImmediate(shotObject);
var boxes = new System.Collections.Generic.List<Midterm.ItemBoxTile>();
foreach (var kind in new[]{"Heart", "Fire"})
{
    var flower = new UnityEngine.GameObject(kind + "Flower", typeof(UnityEngine.SpriteRenderer), typeof(UnityEngine.BoxCollider2D), typeof(Midterm.FlowerPickup));
    var flowerVisual = flower.GetComponent<UnityEngine.SpriteRenderer>();
    flowerVisual.sprite = Sprite("Assets/Sprite/Item/" + kind + "Flower.png");
    flowerVisual.sortingOrder = 11;
    flower.GetComponent<UnityEngine.BoxCollider2D>().isTrigger = true;
    flower.GetComponent<UnityEngine.BoxCollider2D>().size = new UnityEngine.Vector2(.8f, 1f);
    var flowerSo = new UnityEditor.SerializedObject(flower.GetComponent<Midterm.FlowerPickup>());
    flowerSo.FindProperty("kind").enumValueIndex = kind == "Heart" ? 0 : 1;
    flowerSo.ApplyModifiedPropertiesWithoutUndo();
    var flowerPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(flower, root + "/" + kind + "Flower.prefab");
    UnityEngine.Object.DestroyImmediate(flower);
    var box = new UnityEngine.GameObject(kind + "FlowerBox", typeof(UnityEngine.SpriteRenderer), typeof(UnityEngine.BoxCollider2D), typeof(Midterm.ItemBox));
    box.GetComponent<UnityEngine.SpriteRenderer>().sprite = golden;
    box.GetComponent<UnityEngine.SpriteRenderer>().sortingOrder = 12;
    box.GetComponent<UnityEngine.BoxCollider2D>().size = UnityEngine.Vector2.one;
    Set(box.GetComponent<Midterm.ItemBox>(), "itemPrefab", flowerPrefab.GetComponent<Midterm.FlowerPickup>());
    Set(box.GetComponent<Midterm.ItemBox>(), "usedSprite", used);
    var boxPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(box, root + "/" + kind + "FlowerBox.prefab");
    UnityEngine.Object.DestroyImmediate(box);
    var tilePath = "Assets/Tiles/Items/" + kind + "FlowerBox.asset";
    var tile = UnityEditor.AssetDatabase.LoadAssetAtPath<Midterm.ItemBoxTile>(tilePath);
    if (tile == null) { tile = UnityEngine.ScriptableObject.CreateInstance<Midterm.ItemBoxTile>(); UnityEditor.AssetDatabase.CreateAsset(tile, tilePath); }
    tile.sprite = golden;
    tile.gameObject = boxPrefab;
    tile.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.None;
    UnityEditor.EditorUtility.SetDirty(tile);
    boxes.Add(tile);
}
string palettePath = "Assets/Palettes/ItemBoxes.prefab";
if (UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(palettePath) == null)
    UnityEditor.Tilemaps.GridPaletteUtility.CreateNewPalette("Assets/Palettes", "ItemBoxes", UnityEngine.GridLayout.CellLayout.Rectangle, UnityEditor.GridPalette.CellSizing.Manual, UnityEngine.Vector3.one, UnityEngine.GridLayout.CellSwizzle.XYZ);
var palette = UnityEditor.PrefabUtility.LoadPrefabContents(palettePath);
try
{
    var map = palette.GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>();
    map.SetTile(UnityEngine.Vector3Int.zero, boxes[0]);
    map.SetTile(UnityEngine.Vector3Int.right, boxes[1]);
    UnityEditor.PrefabUtility.SaveAsPrefabAsset(palette, palettePath);
}
finally { UnityEditor.PrefabUtility.UnloadPrefabContents(palette); }
// Preserve the user's current edits before visiting the other scene.
UnityEditor.AssetDatabase.SaveAssets();
var originalScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
var playerPrefab = UnityEditor.PrefabUtility.LoadPrefabContents("Assets/Prefabs/Player.prefab");
try
{
    var attack = playerPrefab.GetComponent<Midterm.PlayerFireAttack>();
    if (attack == null) attack = playerPrefab.AddComponent<Midterm.PlayerFireAttack>();
    Set(attack, "projectilePrefab", shotPrefab.GetComponent<Midterm.FireballProjectile>());
    UnityEditor.PrefabUtility.SaveAsPrefabAsset(playerPrefab, "Assets/Prefabs/Player.prefab");
}
finally { UnityEditor.PrefabUtility.UnloadPrefabContents(playerPrefab); }
foreach (var path in new[]{"Assets/Scenes/Stage1.unity", "Assets/Scenes/Stage2.unity"})
{
    var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
    var player = UnityEngine.Object.FindFirstObjectByType<Midterm.PlayerController2D>();
    var attack = player.GetComponent<Midterm.PlayerFireAttack>();
    if (attack == null) attack = UnityEditor.Undo.AddComponent<Midterm.PlayerFireAttack>(player.gameObject);
    Set(attack, "projectilePrefab", shotPrefab.GetComponent<Midterm.FireballProjectile>());
    var grid = UnityEngine.Object.FindFirstObjectByType<UnityEngine.Grid>();
    var child = grid.transform.Find("ItemBoxes");
    UnityEngine.Tilemaps.Tilemap map;
    if (child == null)
    {
        var go = new UnityEngine.GameObject("ItemBoxes", typeof(UnityEngine.Tilemaps.Tilemap), typeof(UnityEngine.Tilemaps.TilemapRenderer));
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Add flower boxes");
        go.transform.SetParent(grid.transform, false);
        map = go.GetComponent<UnityEngine.Tilemaps.Tilemap>();
        go.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>().sortingOrder = 12;
    }
    else map = child.GetComponent<UnityEngine.Tilemaps.Tilemap>();
    // Put the examples over supported terrain close to the spawn, without replacing tiles.
    var grounds = UnityEngine.Object.FindObjectsByType<UnityEngine.Tilemaps.Tilemap>(UnityEngine.FindObjectsSortMode.None).Where(m => m.name.StartsWith("Ground_")).ToArray();
    for (int i = 0; i < 2; i++)
    {
        int x = UnityEngine.Mathf.FloorToInt(player.transform.position.x) + 2 + i * 3;
        int floor = int.MinValue;
        for (int y = UnityEngine.Mathf.FloorToInt(player.transform.position.y); y >= -12; y--)
            if (grounds.Any(g => g.HasTile(new UnityEngine.Vector3Int(x, y, 0)))) { floor = y; break; }
        if (floor == int.MinValue) throw new System.Exception("No supported example location at x=" + x);
        var cell = new UnityEngine.Vector3Int(x, floor + 4, 0);
        if (grounds.Any(g => g.HasTile(cell))) throw new System.Exception("Example cell is occupied.");
        if (!map.HasTile(cell)) map.SetTile(cell, UnityEditor.AssetDatabase.LoadAssetAtPath<Midterm.ItemBoxTile>("Assets/Tiles/Items/" + (i == 0 ? "Heart" : "Fire") + "FlowerBox.asset"));
    }
    UnityEditor.EditorUtility.SetDirty(map);
    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
}
UnityEditor.AssetDatabase.SaveAssets();
UnityEditor.SceneManagement.EditorSceneManager.OpenScene(originalScene);
return "Created flower/projectile/box prefabs, ItemBoxes palette and wired both stages. Restored " + originalScene;

