using UnityEngine;
using UnityEngine.Tilemaps;

namespace Midterm
{
    public sealed class SpikeTile : PrefabTile
    {
        // AiCode -------
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
            tileData.flags |= TileFlags.InstantiateGameObjectRuntimeOnly;

            if (Application.isPlaying)
            {
                tileData.sprite = null;
            }
        }
        //-------------
    }
}
