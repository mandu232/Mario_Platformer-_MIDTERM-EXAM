// Run explicitly through Unity eval_file; authoring only, outside Assets compilation.
if (UnityEditor.EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/Stage1.unity")
    throw new System.InvalidOperationException("Open Stage1 in Edit Mode first.");
UnityEngine.Tilemaps.Tilemap Map(string name) => UnityEngine.GameObject.Find(name).GetComponent<UnityEngine.Tilemaps.Tilemap>();
UnityEngine.Tilemaps.TileBase Tile(string path) => UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.TileBase>(path);
var ground = Map("Ground_0");
var background = Map("BackGround");
var water = Map("Water");
var coins = Map("Coins");
var enemies = Map("Enemies");
var checkpoints = Map("Checkpoints");
var spikes = Map("Spikes");
var pads = Map("JumpPads");
var decoration = Map("Decoration");
foreach (var map in new[] {ground,background,water,coins,enemies,checkpoints,spikes,pads,decoration})
    UnityEditor.Undo.RegisterCompleteObjectUndo(map, "Extend Stage1 snow region");
var snow = Tile("Assets/Sprite/Tilemap/Tiles/Snow_Ground.asset");
var coin = Tile("Assets/Sprite/Tilemap/Tiles/Coin/Coin.asset");
var flag = Tile("Assets/Sprite/Tilemap/Tiles/Checkpoint/Checkpoint.asset");
var spike = Tile("Assets/Sprite/Tilemap/Tiles/Spike/Spike.asset");
var pad = Tile("Assets/Tiles/JumpPad.asset");
var waterTop = Tile("Assets/Sprite/Tilemap/Tiles/Water.asset");
var waterFill = Tile("Assets/Sprite/Tilemap/Tiles/Water_1.asset");
if (snow == null || coin == null || flag == null || pad == null) throw new System.Exception("Missing required tiles.");
int[,] segments = { {90,103,-1}, {106,119,0}, {123,138,1}, {141,151,0}, {154,169,2}, {173,187,1}, {190,209,0} };
for (int x=90;x<=222;x++)
{
    for (int y=-12;y<=19;y++)
    {
        if (x<=209)
        {
            foreach (var map in new[] {ground,coins,enemies,checkpoints,spikes,pads,decoration}) map.SetTile(new UnityEngine.Vector3Int(x,y,0),null);
            if (y<=-6) water.SetTile(new UnityEngine.Vector3Int(x,y,0), y==-6?waterTop:waterFill);
        }
        if (x>=116) background.SetTile(new UnityEngine.Vector3Int(x,y,0),background.GetTile(new UnityEngine.Vector3Int(80+(x%4),y,0)));
    }
}
for (int s=0;s<segments.GetLength(0);s++)
{
    int left=segments[s,0], right=segments[s,1], top=segments[s,2];
    for (int x=left;x<=right;x++)
        for (int y=-12;y<=top;y++) ground.SetTile(new UnityEngine.Vector3Int(x,y,0),snow);
    for (int x=left+2;x<right-1;x+=3) coins.SetTile(new UnityEngine.Vector3Int(x,top+3,0),coin);
}
foreach (var cell in new[]{new UnityEngine.Vector3Int(96,0,0),new UnityEngine.Vector3Int(143,1,0),new UnityEngine.Vector3Int(177,2,0),new UnityEngine.Vector3Int(198,1,0)}) checkpoints.SetTile(cell,flag);
foreach (var cell in new[]{new UnityEngine.Vector3Int(101,0,0),new UnityEngine.Vector3Int(149,1,0),new UnityEngine.Vector3Int(185,2,0)}) pads.SetTile(cell,pad);
string EnemyPath(string name) => "Assets/Sprite/Tilemap/Tiles/Enemies/"+name+".asset";
string[] kinds={"Small_BlueEnemy","LargeBlueEnemy","SpikedEnemy","Small_BlueEnemy","LargeBlueEnemy","SpikedEnemy"};
int[,] locations={{112,1},{128,2},{146,1},{161,3},{180,2},{202,1}};
for(int i=0;i<kinds.Length;i++) enemies.SetTile(new UnityEngine.Vector3Int(locations[i,0],locations[i,1],0),Tile(EnemyPath(kinds[i])));
foreach(var cell in new[]{new UnityEngine.Vector3Int(117,4,0),new UnityEngine.Vector3Int(167,6,0),new UnityEngine.Vector3Int(193,4,0)}) enemies.SetTile(cell,Tile(EnemyPath("FlyingEnemy")));
foreach(var cell in new[]{new UnityEngine.Vector3Int(136,6,0),new UnityEngine.Vector3Int(183,6,0)}) enemies.SetTile(cell,Tile(EnemyPath("DroppingEnemy")));
foreach(var cell in new[]{new UnityEngine.Vector3Int(132,2,0),new UnityEngine.Vector3Int(133,2,0),new UnityEngine.Vector3Int(164,3,0),new UnityEngine.Vector3Int(165,3,0)}) spikes.SetTile(cell,spike);
// Reward arcs across the six small gaps.
for(int s=0;s<segments.GetLength(0)-1;s++)
    for(int x=segments[s,1]+1;x<segments[s+1,0];x++) coins.SetTile(new UnityEngine.Vector3Int(x,System.Math.Max(segments[s,2],segments[s+1,2])+3,0),coin);
var spruce=Tile("Assets/Tiles/SnowPine.asset");
if(spruce!=null) foreach(var cell in new[]{new UnityEngine.Vector3Int(92,0,0),new UnityEngine.Vector3Int(108,1,0),new UnityEngine.Vector3Int(125,2,0),new UnityEngine.Vector3Int(156,3,0),new UnityEngine.Vector3Int(175,2,0),new UnityEngine.Vector3Int(195,1,0),new UnityEngine.Vector3Int(206,1,0)}) decoration.SetTile(cell,spruce);
var exit=UnityEngine.Object.FindFirstObjectByType<Midterm.StageExit>();
UnityEditor.Undo.RecordObject(exit.transform,"Move exit to snow goal");
exit.transform.position=new UnityEngine.Vector3(208.5f,1.5f,0);
UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(exit.transform);
foreach(var map in new[]{ground,background,water,coins,enemies,checkpoints,spikes,pads,decoration}) map.RefreshAllTiles();
var collider=ground.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
if(collider!=null)collider.ProcessTilemapChanges();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(ground.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(ground.gameObject.scene);
return new { start=90,end=209,segments=7,exit=exit.transform.position.ToString(),saved=!ground.gameObject.scene.isDirty };
