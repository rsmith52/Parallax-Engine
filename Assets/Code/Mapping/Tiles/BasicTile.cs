using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;
using Utilities;

namespace Mapping.Tiles
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
            if (terrain_tag == TerrainTags.NoPassHelper && Application.isPlaying)
                tile_data.sprite = null;
            else
                tile_data.sprite = sprite;
        }

        public override bool StartUp(Vector3Int pos, ITilemap tilemap, GameObject go)
        {
            Tilemap map = tilemap.GetComponent<Tilemap>();
            
            // Trans tile handling
            if (is_light_trans)
                map.SetColor(pos, new Color(1,1,1,Constants.LIGHT_TRANS_TILE_ALPHA));
            else if (is_trans)
                map.SetColor(pos, new Color(1,1,1,Constants.TRANS_TILE_ALPHA));

            return true;
        }

        public void ExpandTile(Vector3Int pos, Tilemap map)
        {
            // Handle multi-tile tiles
            if (multi_tile)
            {
                BasicTile tile_copy = MultiTileCopy(this);
                Map map_obj = map.GetComponentInParent<Map>();
                Tilemap copy_to_map;
                Tilemap layer_map = map.GetComponentsInParent<Tilemap>().Last();
                if (layer_map == null) return; // error case of object layer existing without parent layer
                
                for (int i = 0; i < tile_size.x_width; i++)
                {
                    for (int j = 0; j < tile_size.y_height; j++)
                    {
                        for (int k = 0; k < tile_size.z_layers; k++)
                        {
                            if (k == 0)
                            {
                                if (i == 0 && j == 0) continue;
                                map.SetTile(pos + (i * Vector3Int.right) + (j * Vector3Int.up), tile_copy);
                            }
                            else
                            {
                                copy_to_map = GetCopyToMap(map_obj, layer_map, k);
                                if (copy_to_map == null) 
                                {
                                    // Debug.Log("Object Layer Map Not Found to Expand: " + this.name);
                                    break;
                                }
                                else
                                {
                                    Vector3Int y_offset = new Vector3Int(0, k, 0);
                                    copy_to_map.SetTile(pos + (i * Vector3Int.right) + (j * Vector3Int.up) + y_offset, tile_copy);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
