using UnityEngine;
using UnityEngine.Tilemaps;

namespace Midterm
{
    public sealed class CoinTile : PrefabTile
    {
        // AiCode -------
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);

            tileData.flags |= TileFlags.InstantiateGameObjectRuntimeOnly;

            //프리팹 미리보기 타일 이미지
            if (Application.isPlaying) 
            {
                tileData.sprite = null;
            }
        }
        //-------


        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instance)
        {
            base.StartUp(position, tilemap, instance);
            if (Application.isPlaying && instance != null)
            {
                instance.GetComponent<CoinPickup>().BindCell(tilemap.GetComponent<Tilemap>(), position);
            }

            return true;
        }
    }
}
