using UnityEngine;
using UnityEngine.Tilemaps;


namespace Midterm
{
    public sealed class JumpPadTile : PrefabTile
    {
        // AiCode -------
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);

            tileData.colliderType = ColliderType.None;
            tileData.flags |= TileFlags.InstantiateGameObjectRuntimeOnly;


            if (Application.isPlaying)
            {
                tileData.sprite = null;
            }
        }
        //-------
    }
}
