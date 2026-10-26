using UnityEngine;
using UnityEngine.Tilemaps;

namespace Midterm
{
    public sealed class EnemyTile : PrefabTile
    {
        // AiCode -------
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
            tileData.colliderType = ColliderType.None;
            tileData.flags |= TileFlags.InstantiateGameObjectRuntimeOnly;


            if (Application.isPlaying) tileData.sprite = null;
        }
        //---------------



        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instance)
        {
            base.StartUp(position, tilemap, instance);
            if (!Application.isPlaying || instance == null)
            {
                return true;
            }


            var map = tilemap.GetComponent<Tilemap>();

            instance.transform.localScale = gameObject.transform.localScale;
            instance.transform.position = map.GetCellCenterWorld(position) + map.transform.TransformVector(transform.GetColumn(3));
            
            var body = instance.GetComponent<Rigidbody2D>();


            if (body != null) body.position = instance.transform.position;

            return true;

        }


    }
}
