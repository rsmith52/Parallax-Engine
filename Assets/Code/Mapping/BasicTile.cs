using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

namespace Mapping
{    
    [CreateAssetMenu(menuName = "Tiles/Basic Tile", fileName = "Basic Tile")]
    [Serializable]
    public class BasicTile : ParallaxTileBase
    {
        #region Editor

        [Title("Basic Tile Settings")]
        public Sprite sprite;

        #endregion


        #region MonoBehavior

        public override void GetTileData(Vector3Int pos, ITilemap tilemap, ref TileData tile_data)
        {
            tile_data.sprite = sprite;
        }

        #endregion
    }
}
