using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;
using Utilities;

namespace Mapping
{    
    [CreateAssetMenu(menuName = "Parallax Tiles/Basic Tile", fileName = "Basic Tile")]
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

        public override bool StartUp(Vector3Int pos, ITilemap tilemap, GameObject go)
        {
            Tilemap map = tilemap.GetComponent<Tilemap>();
            
            // Trans tile handling
            if (is_trans)
                    map.SetColor(pos, new Color(1,1,1,Constants.TRANS_TILE_ALPHA));

            return true;
        }

        #endregion
    }
}
