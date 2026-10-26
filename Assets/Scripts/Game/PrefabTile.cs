using UnityEngine;
using UnityEngine.Tilemaps;

namespace Midterm
{
    // The tile sprite is an editor preview; the spawned prefab owns runtime graphics.
    public abstract class PrefabTile : Tile
    {
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData data)
        {
            base.GetTileData(position, tilemap, ref data);
            data.flags |= TileFlags.InstantiateGameObjectRuntimeOnly;
            if (!Application.isPlaying) return;
            data.sprite = null;
            var color = data.color;
            color.a = 0;
            data.color = color;
            data.flags &= ~TileFlags.LockColor;
        }

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instance)
        {
            if (!Application.isPlaying || instance == null) return true;
            var map = tilemap.GetComponent<Tilemap>();
            // Player builds can load cached editor sprites without recalculating TileData.
            // Clear the cell's rendering independently of that cache; prefab renderers stay visible.
            map.SetTileFlags(position, map.GetTileFlags(position) & ~TileFlags.LockColor);
            var color = map.GetColor(position);
            color.a = 0;
            map.SetColor(position, color);
            return true;
        }
    }
}
