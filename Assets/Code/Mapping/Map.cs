using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;
using Utilities;
using Eventing;

namespace Mapping
{
    #region Enums

    public enum Maps
    {
        None = 0,
        TestMap1 = 1
    }

    public enum MapType
    {
        None = 0,
        Outdoors = 1
    }

    #endregion


    #region Structs

    [Serializable]
    public struct NeighborTilemaps
    {
        public Tilemap ground;
        public Tilemap layer_up;

        public Tilemap[] objects;
        public Tilemap[] objects_up;
    }

    [Serializable]
    public struct NeighborTiles
    {
        public ParallaxTileBase on_tile;
        public ParallaxTileBase up_tile;
        public ParallaxTileBase left_tile;
        public ParallaxTileBase right_tile;
        public ParallaxTileBase down_tile;

        // public ParallaxTileBase above_tile;
        // public ParallaxTileBase below_tile;
    }

    [Serializable]
    public struct MatchedTile
    {
        public ParallaxTileBase tile;
        public Tilemap map;
    }

    #endregion


    [Serializable]
    public class Map : SerializedMonoBehaviour
    {
        #region Global

        public static Dictionary<Maps, Map> map_dict;

        #endregion
        

        #region Fields

        [Title("Basic Information")]
        public int map_id;
        [ValidateInput("NameMatchesGameObject", "Map's name must match the game object's name.")]
        public string map_name;
        public MapType map_type;

        [Title("Layer Information")]
        public Dictionary<int, Tilemap> map_layers;
        [ReadOnly]
        public Dictionary<int, Tilemap[]> object_layers;

        #endregion


        #region Validation Methods

        private bool NameMatchesGameObject()
        {
            return map_name.Equals(this.gameObject.name);
        }

        #endregion


        #region Mono Behavior

        private void Start()
        {
            object_layers = new Dictionary<int, Tilemap[]>();
            foreach (KeyValuePair<int, Tilemap> layer in map_layers)
            {
                int sorting_layer = layer.Value.GetComponent<TilemapRenderer>().sortingOrder;
                object_layers[layer.Key] = layer.Value.GetComponentsInChildren<Tilemap>().Skip(1).ToArray();
                for (int i = 0; i < object_layers[layer.Key].Length; i++)
                {
                    Tilemap object_layer = object_layers[layer.Key][i];
                    
                    // Dynamic set sorting layers
                    TilemapRenderer renderer = object_layer.GetComponent<TilemapRenderer>();
                    renderer.sortingOrder = sorting_layer + i;

                    // Instantiate Prefab Tiles
                    foreach (Vector3Int pos in object_layer.cellBounds.allPositionsWithin)
                    {
                        Vector3Int local_place = new Vector3Int(pos.x, pos.y, pos.z);

                        if (object_layer.HasTile(local_place))
                        {
                            // Get PrefabTile for this tile
                            PrefabTile prefab_tile = object_layer.GetTile<PrefabTile>(local_place);
                            if (!prefab_tile)
                                continue;

                            // Instantiate prefab game object
                            prefab_tile.InstantiatePrefab(local_place, object_layer);
                        }
                    }
                }
            }
        }

        #endregion


        #region Map Awareness
        
        private NeighborTilemaps GetNeighborTileMaps (Vector3 pos)
        {
            NeighborTilemaps neighbor_maps = new NeighborTilemaps{};

            foreach (KeyValuePair<int, Tilemap> layer in map_layers)
            {
                Tilemap map = layer.Value;
                if (map.transform.position.z == pos.z)
                {
                    neighbor_maps.ground = map;
                    neighbor_maps.objects = object_layers[layer.Key];
                }
                else if (map.transform.position.z == pos.z - Constants.MAP_LAYER_HEIGHT)
                {
                    neighbor_maps.layer_up = map;
                    neighbor_maps.objects_up = object_layers[layer.Key];
                }
            }
            
            return neighbor_maps;
        }

        public NeighborTiles GetNeighborTiles (MoveableObject character)
        {
            return GetNeighborTiles(character.transform.position);
        }

        public NeighborTiles GetNeighborTiles (Vector3 pos)
        {
            NeighborTilemaps neighbor_maps = GetNeighborTileMaps(pos);
            NeighborTiles neighbor_tiles = new NeighborTiles{};

            Vector3Int int_pos = new Vector3Int (
                (int)(pos.x),
                (int)(pos.y),
                0
            );

            neighbor_tiles.on_tile = GetTileAtPosition (neighbor_maps, int_pos);
            neighbor_tiles.up_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.up);
            neighbor_tiles.left_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.left);
            neighbor_tiles.right_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.right);
            neighbor_tiles.down_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down);

            return neighbor_tiles;
        }

        private ParallaxTileBase GetTileAtPosition (NeighborTilemaps neighbor_maps, Vector3Int pos)
        {
            MatchedTile matched_tile = new MatchedTile{};
            
            // Check Layer Up
            matched_tile = CheckTilePositionOnLayer(matched_tile, pos, neighbor_maps.layer_up, neighbor_maps.objects_up);
            
            // Check Current Layer
            if (matched_tile.tile == null)
                matched_tile = CheckTilePositionOnLayer(matched_tile, pos, neighbor_maps.ground, neighbor_maps.objects);
            
            // Handle terrain tiles
            if (matched_tile.tile != null)
            {
                RuleTile ruletile = matched_tile.tile as RuleTile;
                if (ruletile != null)
                {
                    matched_tile.tile = ConvertTerrainRuleTile(ruletile, matched_tile.map, pos);
                }
            }
            return matched_tile.tile;
        }

        private MatchedTile CheckTilePositionOnLayer (MatchedTile matched_tile, Vector3Int pos, Tilemap layer, Tilemap[] objects)
        {
            if (matched_tile.tile == null && objects != null)
            {
                for (int i = objects.Length - 1; i >= 0; i--)
                {
                    if (matched_tile.tile)
                        break;
                    matched_tile.tile = (ParallaxTileBase)objects[i].GetTile(pos);
                    matched_tile.map = objects[i];
                }
            }
            if (matched_tile.tile == null && layer != null)
            {
                matched_tile.tile = (ParallaxTileBase)layer.GetTile(pos);
                matched_tile.map = layer;
            }

            return matched_tile;
        }

        private ParallaxTileBase ConvertTerrainRuleTile(RuleTile ruletile, Tilemap matched_map, Vector3Int pos)
        {
            if (!ruletile.is_terrain) return ruletile;
            string tile_name = ruletile.name;
            ParallaxTileBase check_tile;

            // Check Left
            check_tile = (ParallaxTileBase)matched_map.GetTile(pos + Vector3Int.left);
            if (check_tile == null || check_tile.name != tile_name) return ruletile;
            
            // Check Right
            check_tile = (ParallaxTileBase)matched_map.GetTile(pos + Vector3Int.right);
            if (check_tile == null || check_tile.name != tile_name) return ruletile;

            // Check Down
            check_tile = (ParallaxTileBase)matched_map.GetTile(pos + Vector3Int.down);
            if (check_tile == null || check_tile.name != tile_name) return ruletile;

            // Check Lower Corners
            check_tile = (ParallaxTileBase)matched_map.GetTile(pos + Vector3Int.down + Vector3Int.left);
            if (check_tile == null || check_tile.name != tile_name) return ruletile;
            check_tile = (ParallaxTileBase)matched_map.GetTile(pos + Vector3Int.down + Vector3Int.right);
            if (check_tile == null || check_tile.name != tile_name) return ruletile;

            // Check Up
            check_tile = (ParallaxTileBase)matched_map.GetTile(pos + Vector3Int.up);
            if (check_tile == null || check_tile.name != tile_name) return ruletile.surface_edge;

            // Check Upper Corners
            check_tile = (ParallaxTileBase)matched_map.GetTile(pos + Vector3Int.up + Vector3Int.left);
            if (check_tile == null || check_tile.name != tile_name) return ruletile.surface_edge;
            check_tile = (ParallaxTileBase)matched_map.GetTile(pos + Vector3Int.up + Vector3Int.right);
            if (check_tile == null || check_tile.name != tile_name) return ruletile.surface_edge;

            return ruletile.surface_tile;
        }

        #endregion


        #region Static Methods

        public static Map getMapByID(int id)
        {
            return map_dict[(Maps)id];
        }

        #endregion
    }
}