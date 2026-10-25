// Explicit Editor authoring recipe, never auto-executed.
if(UnityEditor.EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="Stage1") throw new System.Exception("Open Stage1 in Edit Mode.");
UnityEngine.Tilemaps.Tilemap Map(string n)=>UnityEngine.GameObject.Find(n).GetComponent<UnityEngine.Tilemaps.Tilemap>();
UnityEngine.Tilemaps.TileBase Asset(string p)=>UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.TileBase>(p);
UnityEngine.Tilemaps.Tilemap Layer(string name,int order,UnityEngine.Color color){var go=UnityEngine.GameObject.Find(name);if(go==null){go=new UnityEngine.GameObject(name,typeof(UnityEngine.Tilemaps.Tilemap),typeof(UnityEngine.Tilemaps.TilemapRenderer));UnityEditor.Undo.RegisterCreatedObjectUndo(go,"Sculpt snow landscape");go.transform.SetParent(UnityEngine.GameObject.Find("Grid").transform,false);}var m=go.GetComponent<UnityEngine.Tilemaps.Tilemap>();m.color=color;go.GetComponent<UnityEngine.Tilemaps.TilemapRenderer>().sortingOrder=order;return m;}
var ground=Map("Ground_0");var coins=Map("Coins");var enemies=Map("Enemies");var flags=Map("Checkpoints");var pads=Map("JumpPads");var spikes=Map("Spikes");var decor=Map("Decoration");
var distant=Layer("SnowDistantRidges",1,new UnityEngine.Color(.55f,.73f,.8f,.65f));
var near=Layer("SnowNearRidges",2,new UnityEngine.Color(.46f,.61f,.68f,.8f));
var cave=Layer("SnowCaveWall",3,new UnityEngine.Color(.36f,.4f,.48f,1));
foreach(var m in new[]{ground,coins,enemies,flags,pads,spikes,decor,distant,near,cave}){UnityEditor.Undo.RegisterCompleteObjectUndo(m,"Sculpt snow landscape");for(int x=90;x<=222;x++)for(int y=-12;y<=17;y++)m.SetTile(new UnityEngine.Vector3Int(x,y,0),null);}
var snow=Asset("Assets/Sprite/Tilemap/Tiles/Snow_Ground.asset");
var coin=Asset("Assets/Sprite/Tilemap/Tiles/Coin/Coin.asset");var flag=Asset("Assets/Sprite/Tilemap/Tiles/Checkpoint/Checkpoint.asset");var spike=Asset("Assets/Sprite/Tilemap/Tiles/Spike/Spike.asset");var pad=Asset("Assets/Tiles/JumpPad.asset");var pine=Asset("Assets/Tiles/SnowPine.asset");
var sprites=UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite/Tilemap/tilemap.png");
UnityEngine.Tilemaps.Tile Simple(string name,int index){string p="Assets/Tiles/"+name+".asset";var t=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.Tile>(p);if(t==null){t=UnityEngine.ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();t.sprite=(UnityEngine.Sprite)System.Array.Find(sprites,o=>o.name=="tilemap_"+index);t.colliderType=UnityEngine.Tilemaps.Tile.ColliderType.Grid;UnityEditor.AssetDatabase.CreateAsset(t,p);}return t;}
var bridge=Simple("SnowBridge",49);var rock=Simple("SnowRock",148);
// Main route: forest ascent, sheltered ravine, mountain ridge, descent, summit gate.
int[,] route={{90,97,0},{98,103,1},{104,109,2},{112,119,0},{120,125,-1},{126,132,-2},{133,139,-1},{140,145,1},{146,148,0},{149,155,2},{156,161,3},{162,167,5},{168,173,6},{176,183,4},{184,189,2},{190,195,1},{196,201,2},{202,209,3}};
for(int s=0;s<route.GetLength(0);s++)for(int x=route[s,0];x<=route[s,1];x++)for(int y=-12;y<route[s,2];y++)ground.SetTile(new UnityEngine.Vector3Int(x,y,0),snow);
void Shelf(int l,int r,int floor,int depth){for(int x=l;x<=r;x++)for(int y=floor-depth;y<floor;y++)ground.SetTile(new UnityEngine.Vector3Int(x,y,0),snow);}
Shelf(112,115,3,1);Shelf(118,138,5,2);Shelf(140,143,7,2);Shelf(151,156,7,2);
// Raised wooden bridge crosses above the lower jumping route.
for(int x=144;x<=150;x++)ground.SetTile(new UnityEngine.Vector3Int(x,5,0),bridge);
Shelf(179,182,8,1);Shelf(186,188,6,1);Shelf(195,198,6,1);
// Layered mountain silhouettes stay behind all traversable geometry.
for(int x=90;x<=222;x++){
 int peak=5+(int)(6*System.Math.Max(0,1-System.Math.Abs((x-90)%31-15)/15f));
 for(int y=-7;y<=peak;y++)distant.SetTile(new UnityEngine.Vector3Int(x,y,0),snow);
 int middle=2+(int)(5*System.Math.Max(0,1-System.Math.Abs((x-90)%23-11)/11f));
 for(int y=-7;y<=middle;y++)near.SetTile(new UnityEngine.Vector3Int(x,y,0),snow);
}
for(int x=118;x<=138;x++)for(int y=-4;y<=4;y++)cave.SetTile(new UnityEngine.Vector3Int(x,y,0),snow);
void Trail(int l,int r,int y,int stride){for(int x=l;x<=r;x+=stride)coins.SetTile(new UnityEngine.Vector3Int(x,y,0),coin);}
Trail(92,108,3,3);Trail(119,137,6,2);Trail(141,156,8,2);Trail(121,137,0,3);Trail(151,159,4,3);Trail(164,172,8,2);Trail(179,181,9,1);Trail(187,197,7,2);Trail(200,206,5,2);
foreach(var cell in new[]{new UnityEngine.Vector3Int(110,4,0),new UnityEngine.Vector3Int(111,4,0),new UnityEngine.Vector3Int(146,4,0),new UnityEngine.Vector3Int(147,4,0),new UnityEngine.Vector3Int(148,4,0),new UnityEngine.Vector3Int(174,8,0),new UnityEngine.Vector3Int(175,8,0)})coins.SetTile(cell,coin);
foreach(var cell in new[]{new UnityEngine.Vector3Int(95,0,0),new UnityEngine.Vector3Int(135,-1,0),new UnityEngine.Vector3Int(158,3,0),new UnityEngine.Vector3Int(192,1,0)})flags.SetTile(cell,flag);
foreach(var cell in new[]{new UnityEngine.Vector3Int(114,0,0),new UnityEngine.Vector3Int(145,1,0),new UnityEngine.Vector3Int(177,4,0)})pads.SetTile(cell,pad);
string EP(string n)=>"Assets/Sprite/Tilemap/Tiles/Enemies/"+n+".asset";
string[] kinds={"Small_BlueEnemy","LargeBlueEnemy","SpikedEnemy","Small_BlueEnemy","LargeBlueEnemy","SpikedEnemy","FlyingEnemy","FlyingEnemy","FlyingEnemy","DroppingEnemy","DroppingEnemy"};
int[,] spots={{101,1},{125,5},{129,-2},{153,7},{165,5},{200,2},{109,6},{148,10},{185,9},{123,2},{137,2}};
for(int i=0;i<kinds.Length;i++)enemies.SetTile(new UnityEngine.Vector3Int(spots[i,0],spots[i,1],0),Asset(EP(kinds[i])));
foreach(var cell in new[]{new UnityEngine.Vector3Int(127,-2,0),new UnityEngine.Vector3Int(128,-2,0),new UnityEngine.Vector3Int(155,2,0),new UnityEngine.Vector3Int(170,6,0),new UnityEngine.Vector3Int(171,6,0),new UnityEngine.Vector3Int(188,2,0)})spikes.SetTile(cell,spike);
foreach(var cell in new[]{new UnityEngine.Vector3Int(91,0,0),new UnityEngine.Vector3Int(94,0,0),new UnityEngine.Vector3Int(99,1,0),new UnityEngine.Vector3Int(107,2,0),new UnityEngine.Vector3Int(119,5,0),new UnityEngine.Vector3Int(132,5,0),new UnityEngine.Vector3Int(142,7,0),new UnityEngine.Vector3Int(160,3,0),new UnityEngine.Vector3Int(168,6,0),new UnityEngine.Vector3Int(182,4,0),new UnityEngine.Vector3Int(191,1,0),new UnityEngine.Vector3Int(204,3,0),new UnityEngine.Vector3Int(207,3,0)})decor.SetTile(cell,pine);
foreach(var cell in new[]{new UnityEngine.Vector3Int(96,0,0),new UnityEngine.Vector3Int(106,2,0),new UnityEngine.Vector3Int(121,-1,0),new UnityEngine.Vector3Int(134,-1,0),new UnityEngine.Vector3Int(163,5,0),new UnityEngine.Vector3Int(181,4,0),new UnityEngine.Vector3Int(205,3,0)})decor.SetTile(cell,rock);
var exit=UnityEngine.Object.FindFirstObjectByType<Midterm.StageExit>();UnityEditor.Undo.RecordObject(exit.transform,"Move summit exit");exit.transform.position=new UnityEngine.Vector3(208.5f,3.5f,0);UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(exit.transform);
foreach(var m in new[]{ground,coins,enemies,flags,pads,spikes,decor,distant,near,cave})m.RefreshAllTiles();ground.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>().ProcessTilemapChanges();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(ground.gameObject.scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(ground.gameObject.scene);UnityEditor.AssetDatabase.SaveAssets();return "Sculpted 17 main terraces, cave/roof branches, upper bridge, summit and layered scenery.";
