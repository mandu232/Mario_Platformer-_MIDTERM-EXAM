using UnityEngine;
using UnityEngine.Tilemaps;

namespace Midterm
{
    public sealed class BreakableBlockTile : PrefabTile
    {
        // AiCode -------
        public override void GetTileData(Vector3Int position, ITilemap map, ref TileData data)
        {
            base.GetTileData(position, map, ref data);

            data.flags |= TileFlags.InstantiateGameObjectRuntimeOnly;

            if (Application.isPlaying) 
            {
                data.sprite = null;
            }
        }




        public override bool StartUp(Vector3Int position, ITilemap map, GameObject instance)
        {
            base.StartUp(position, map, instance);
            if (Application.isPlaying && instance != null)
            {
                instance.GetComponent<BreakableBlock>().BindCell(map.GetComponent<Tilemap>(), position);
            }
            
            return true;
        }
        //-------------
    }
}
