using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;
using Utilities;

namespace Mapping
{
    #region Structs

    [Serializable]
    public struct TileSize
    {
        public int x_width;
        public int y_height;
        public int z_layers;
    }

    #endregion


    [CreateAssetMenu(menuName = "Parallax Tiles/Prefab Tile", fileName = "Prefab Tile")]
    [Serializable]
    public class PrefabTile : ParallaxTileBase
    {
        #region Editor
        
        [Title("Prefab Tile Settings")]
        public Sprite preview_sprite;
        public GameObject prefab;

        [DetailedInfoBox("Offset Considerations", "If the tile has an offset in it's tile palette, this value should be the intended offset (default 0.5, 0.5) + that grid selection transform offset.")]
        public Vector2 prefab_offset = new Vector2(0.5f, 0.5f);
        public float prefab_local_z = 0;

        #endregion


        #region MonoBehavior

        public override void GetTileData(Vector3Int pos, ITilemap tilemap, ref TileData tile_data)
        {
            tile_data.sprite = preview_sprite;
            Tilemap map = tilemap.GetComponent<Tilemap>();
            tile_data.gameObject = Application.isPlaying ? null : prefab;

            if (!IsTilemapFromPalette(map))
            {
                tile_data.sprite = null;
            }
        }

        /*
        * Fix display in scene editor to not show rogue prefabs.
        */
        public override bool StartUp(Vector3Int pos, ITilemap tilemap, GameObject go)
        {
            // This prevents rogue prefab objects from appearing when the Tile palette is present
            #if UNITY_EDITOR
            if (go != null)
            {
                if (go.scene.name == null)
                    DestroyImmediate(go);
            }
            #endif

            if (go != null)
            {
                // Modify position of game object to match middle of Tile sprite
                go.transform.position = new Vector3(
                    pos.x + prefab_offset.x
                    , pos.y + prefab_offset.y
                    , pos.z);
                
                // Set proper Z Offset
                go.transform.localPosition = new Vector3(go.transform.localPosition.x,
                    go.transform.localPosition.y, prefab_local_z);

                SpriteUtils.ConfigurePrefabTileSprites(tilemap.GetComponent<Tilemap>(), go, is_trans, is_light_trans);
            }

            return true;
        }

        /*
        * Instantiate prefab in runtime.
        */
        public void InstantiatePrefab(Vector3Int pos, Tilemap map)
        {   
            GameObject instance = Instantiate(prefab);
            instance.transform.SetParent(map.transform);
            instantiated_object = instance;

            // Modify position of object to match middle of tile sprite
            instance.transform.position = new Vector3(
                pos.x + prefab_offset.x,
                pos.y + prefab_offset.y,
                pos.z
            );

            // Set proper Z Offset
            instance.transform.localPosition = new Vector3(instance.transform.localPosition.x,
                instance.transform.localPosition.y, prefab_local_z);

            SpriteUtils.ConfigurePrefabTileSprites(map, instance, is_trans, is_light_trans);

            // Handle multi-tile prefabs
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