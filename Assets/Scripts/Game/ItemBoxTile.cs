using UnityEngine;
using UnityEngine.Tilemaps;

namespace Midterm
{
    public sealed class ItemBoxTile : PrefabTile
    {
        // AiCode -------
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData data)
        {

            base.GetTileData(position, tilemap, ref data);

            data.colliderType = ColliderType.None;
            data.flags |= TileFlags.InstantiateGameObjectRuntimeOnly;

            if (Application.isPlaying)
            {
                data.sprite = null;
            }


        }
        //-----------




        // AiCode -------
        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instance)
        {
            base.StartUp(position, tilemap, instance);
            
            if (Application.isPlaying && instance != null)
            {
                instance.transform.position = tilemap.GetComponent<Tilemap>().GetCellCenterWorld(position);
            }

            return true;
        }
        //-----------
    }
}
