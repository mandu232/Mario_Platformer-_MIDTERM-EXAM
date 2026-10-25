using UnityEngine;
using UnityEngine.Tilemaps;

namespace Midterm
{
    public sealed class StageExitTile : Tile
    {
        // AiCode -------
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
            // 문 프리팹이 위아래 두 칸을 함께 표시한다.
            // 아래 칸 하나를 칠하거나 지우면 문 전체가 함께 처리된다.
            tileData.sprite = null;
            tileData.colliderType = ColliderType.None;
            tileData.flags = TileFlags.LockAll;
        }
        // AiCode -------

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instance)
        {
            if (Application.isPlaying && instance != null)
                instance.GetComponent<StageExit>().ConfigureNextStage();
            return true;
        }
    }
}
