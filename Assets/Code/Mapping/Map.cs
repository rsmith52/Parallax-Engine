using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;
using Utilities;
using Eventing;
using Sherbert.Framework.Generic;

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
        Outdoors = 1,
    }

    #endregion


    #region Structs

    [Serializable]
    public struct NeighborTilemaps
    {
        public Tilemap ground;
        public Tilemap layer_up;
        public Tilemap layer_down;

        public Tilemap[] objects;
        public Tilemap[] objects_up;
        public Tilemap[] objects_down;
    }

    [Serializable]
    public struct NeighborTiles
    {
        // Immediate Neighbors
        public ParallaxTileBase on_tile;
        public ParallaxTileBase up_tile;
        public ParallaxTileBase left_tile;
        public ParallaxTileBase right_tile;
        public ParallaxTileBase down_tile;

        // Facing Forward
        public ParallaxTileBase facing_tile;
        public ParallaxTileBase look_ahead_tile;
        [HideInInspector]
        public ParallaxTileBase look_ahead_cw_tile;
        [HideInInspector]
        public ParallaxTileBase look_ahead_ccw_tile;

        // Layers Above/Below
        public bool facing_other_level;
        public bool look_ahead_other_level;
        public ParallaxTileBase above_tile;
        public ParallaxTileBase below_tile;

        // Rarely used Extra Awareness
        [HideInInspector]
        public ParallaxTileBase up_left_tile;
        [HideInInspector]
        public ParallaxTileBase up_right_tile;
        [HideInInspector]
        public ParallaxTileBase down_left_tile;
        [HideInInspector]
        public ParallaxTileBase down_right_tile;
    }

    [Serializable]
    public struct MatchedTile
    {
        public ParallaxTileBase tile;
        public Tilemap map;
        public Tilemap layer;
        public bool object_match;
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
        public SerializableDictionary<int, Tilemap> map_layers;
        [ReadOnly]
        public Dictionary<int, Tilemap[]> object_layers;

        #endregion


        #region Validation Methods

        private bool NameMatchesGameObject()
        {
            return map_name.Equals(this.gameObject.name);
        }

        #endregion


        #region Map Creation

        [TitleGroup("Expand Map")]
        [HorizontalGroup("Expand Map/Split")]
        [VerticalGroup("Expand Map/Split/Left")]
        [BoxGroup("Expand Map/Split/Left/Add Layers")]
        [Button("+ Bottom Layer")]
        public void AddNewBottomLayer()
        {
            int lowest_layer = 1;
            foreach (int key in map_layers.Keys)
                lowest_layer = (key < lowest_layer) ? key : lowest_layer;
                
            CreateNewMapLayer(lowest_layer - 1);
        }

        [BoxGroup("Expand Map/Split/Left/Add Layers")]
        [Button("+ Top Layer")]
        public void AddNewTopLayer()
        {
            int highest_layer = -1;
            foreach (int key in map_layers.Keys)
                highest_layer = (key > highest_layer) ? key : highest_layer;

            CreateNewMapLayer(highest_layer + 1);
        }

        private void CreateNewMapLayer(int layer_id, int num_object_layers = 2)
        {
            GameObject layer = new GameObject("Layer " + layer_id);
            layer.transform.parent = this.transform;
            layer.transform.position = new Vector3(0, 0, -1 * layer_id * Constants.MAP_LAYER_HEIGHT);
            if (layer_id < 0)
                layer.transform.SetSiblingIndex(0);
            
            layer.AddComponent<Tilemap>();
            layer.AddComponent<TilemapRenderer>();
            TilemapRenderer renderer = layer.GetComponent<TilemapRenderer>();

            renderer.sortingLayerID = Constants.MAP_SORTING_LAYER_ID;
            renderer.sortingLayerName = Constants.MAP_SORTING_LAYER_NAME;
            renderer.sortingOrder = layer_id * Constants.SORTING_LAYERS_PER_MAP_LAYER;
            renderer.material = SpriteUtils.GetPixelSnappingMaterial();

            map_layers.Add(layer_id, layer.GetComponent<Tilemap>());

            // Ground Layer
            AddNewObjectLayer(layer_id, 0, true);
            for (int i = 0; i < num_object_layers; i++)
            {
                // Object Layers
                AddNewObjectLayer(layer_id, i + 1);
            }
        }
        
        [BoxGroup("Expand Map/Split/Left/Delete Layers")]
        [Button("- Bottom Layer")]
        public void DeleteBottomLayer()
        {
            int lowest_layer = 1;
            foreach (int key in map_layers.Keys)
                lowest_layer = (key < lowest_layer) ? key : lowest_layer;

            DeleteMapLayer(lowest_layer);
        }

        [BoxGroup("Expand Map/Split/Left/Delete Layers")]
        [Button("- Top Layer")]
        public void DeleteTopLayer()
        {
            int highest_layer = -1;
            foreach (int key in map_layers.Keys)
                highest_layer = (key > highest_layer) ? key : highest_layer;

            DeleteMapLayer(highest_layer);
        }

        private void DeleteMapLayer(int layer_id)
        {
            GameObject go = map_layers[layer_id].gameObject;
            map_layers.Remove(layer_id);
            GameObject.DestroyImmediate(go);
        }

        [VerticalGroup("Expand Map/Split/Right")]
        [BoxGroup("Expand Map/Split/Right/Objects")]
        [Button("+ Object Layer")]
        public void AddNewObjectLayer(int layer_id, int object_layer_id, bool is_ground = false)
        {
            string layer_name = is_ground ? "Ground" : "Objects " + layer_id + "." + object_layer_id;
            GameObject object_layer = new GameObject(layer_name);
            object_layer.transform.parent = map_layers[layer_id].transform;
            object_layer.transform.position += new Vector3(0, 0, -1 * layer_id * Constants.MAP_LAYER_HEIGHT);
            
            object_layer.AddComponent<Tilemap>();
            object_layer.AddComponent<TilemapRenderer>();
            TilemapRenderer renderer = object_layer.GetComponent<TilemapRenderer>();
            int layer_sorting_layer = map_layers[layer_id].GetComponent<TilemapRenderer>().sortingOrder;

            renderer.sortingLayerID = Constants.MAP_SORTING_LAYER_ID;
            renderer.sortingLayerName = Constants.MAP_SORTING_LAYER_NAME;
            renderer.sortingOrder = layer_sorting_layer + object_layer_id + Constants.OBJECT_LAYER_START_OFFSET;
            renderer.material = SpriteUtils.GetPixelSnappingMaterial();

            if (is_ground)
                object_layer.tag = Constants.GROUND_LAYER_TAG;
        }

        #endregion


        #region Mono Behavior

        private void Start()
        {
            // Populate Object Layers
            object_layers = new Dictionary<int, Tilemap[]>();
            foreach (KeyValuePair<int, Tilemap> layer in map_layers)
            {
                if (layer.Value == null) break;
                object_layers[layer.Key] = layer.Value.GetComponentsInChildren<Tilemap>().Skip(1).ToArray();
            }

            // Set Sorting Order & Expand Prefab Tiles
            foreach (KeyValuePair<int, Tilemap> layer in map_layers)
            {
                if (layer.Value == null) break;

                TilemapRenderer layer_renderer = layer.Value.GetComponent<TilemapRenderer>();
                layer_renderer.sortingOrder = layer.Key * Constants.SORTING_LAYERS_PER_MAP_LAYER;
                int sorting_layer = layer_renderer.sortingOrder;
                
                for (int i = 0; i < object_layers[layer.Key].Length; i++)
                {
                    Tilemap object_layer = object_layers[layer.Key][i];

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
                    
                    // Dynamic set sorting layers
                    TilemapRenderer renderer = object_layer.GetComponent<TilemapRenderer>();
                    renderer.sortingOrder = sorting_layer + i + Constants.OBJECT_LAYER_START_OFFSET;                        
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
                else if (map.transform.position.z == pos.z + Constants.MAP_LAYER_HEIGHT)
                {
                    neighbor_maps.layer_down = map;
                    neighbor_maps.objects_down = object_layers[layer.Key];
                }
            }
            
            return neighbor_maps;
        }

        public NeighborTiles GetNeighborTiles (MoveableObject character, bool look_only = false, 
                                                bool down_stairs = false, bool up_stairs = false)
        {
            Vector3 pos = character.transform.position;
            if (down_stairs)
                pos += new Vector3(0, 0, 0.5f);
            else if (up_stairs)
                pos += new Vector3(0, 0, -0.5f);

            NeighborTilemaps neighbor_maps = GetNeighborTileMaps(pos);
            NeighborTiles neighbor_tiles = new NeighborTiles{};
            if (look_only)
                neighbor_tiles = character.neighbor_tiles;

            return GetNeighborTiles(pos, character.direction, look_only, neighbor_maps, neighbor_tiles);
        }

        public NeighborTiles GetNeighborTiles (Vector3 pos, Directions direction, bool look_only,
                                                NeighborTilemaps neighbor_maps, NeighborTiles neighbor_tiles)
        {
            bool on_stairs = false;
            bool on_water = false;
            bool on_bridge = false;

            Vector3Int int_pos = new Vector3Int (
                (int)(pos.x),
                (int)(pos.y),
                0
            );

            // Current Tile - modifies some behavior of neighbor tile detection/perception
            MatchedTile on_tile = GetTileAtPosition (neighbor_maps, int_pos);
            neighbor_tiles.on_tile = on_tile.tile;

            if (!look_only)
            {
                if (neighbor_tiles.on_tile != null)
                {
                    on_stairs = (neighbor_tiles.on_tile != null && ParallaxTerrain.IsStairTile(neighbor_tiles.on_tile));
                    on_water = (neighbor_tiles.on_tile != null && ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile));
                    on_bridge = (neighbor_tiles.on_tile != null && ParallaxTerrain.IsBridgeTile(neighbor_tiles.on_tile));
                }
            
                // Same Level Neighbors
                neighbor_tiles.up_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.up, on_stairs).tile;
                neighbor_tiles.left_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.left, on_stairs).tile;
                neighbor_tiles.right_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.right, on_stairs).tile;
                neighbor_tiles.down_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down, on_stairs).tile;

                // Above / Below Level Neighbors - primarily bridge & water use
                neighbor_tiles.above_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.up, on_stairs, true).tile;
                if (on_bridge || on_water)
                    neighbor_tiles.below_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down, on_stairs, false, true).tile;

                // Corner Neighbors
                neighbor_tiles.up_left_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.up + Vector3Int.left, on_stairs).tile;
                neighbor_tiles.up_right_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.up + Vector3Int.right, on_stairs).tile;
                neighbor_tiles.down_left_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down + Vector3Int.left, on_stairs).tile;
                neighbor_tiles.down_right_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down + Vector3Int.right, on_stairs).tile;
            }

            // Facing & Look Ahead Tiles
            Vector3Int dir_vector = new Vector3Int();
            Vector3Int cw_dir_vector = new Vector3Int();
            Vector3Int ccw_dir_vector = new Vector3Int();
            switch (direction)
            {
                case Directions.Up:
                    dir_vector = Vector3Int.up;
                    cw_dir_vector = (2 * Vector3Int.up) + Vector3Int.right;
                    ccw_dir_vector = (2 * Vector3Int.up) + Vector3Int.left;
                    break;
                case Directions.Left:
                    dir_vector = Vector3Int.left;
                    cw_dir_vector = (2 * Vector3Int.left) + Vector3Int.up;
                    ccw_dir_vector = (2 * Vector3Int.left) + Vector3Int.down;
                    break;
                case Directions.Right:
                    dir_vector = Vector3Int.right;
                    cw_dir_vector = (2 * Vector3Int.right) + Vector3Int.down;
                    ccw_dir_vector = (2 * Vector3Int.right) + Vector3Int.up;
                    break;
                case Directions.Down:
                    dir_vector = Vector3Int.down;
                    cw_dir_vector = (2 * Vector3Int.down) + Vector3Int.left;
                    ccw_dir_vector = (2 * Vector3Int.down) + Vector3Int.right;
                    break;
                default:
                    break;
            }

            MatchedTile facing_tile = GetTileAtPosition (neighbor_maps, int_pos + dir_vector, on_stairs);
            neighbor_tiles.facing_tile = facing_tile.tile;
            neighbor_tiles.facing_other_level = ((on_tile.layer == null && facing_tile.layer != null) || (on_tile.layer != null && facing_tile.layer == null) ||
                (on_tile.layer != null && facing_tile.layer != null && on_tile.layer.name != facing_tile.layer.name));

            MatchedTile look_ahead_tile = GetTileAtPosition (neighbor_maps, int_pos + (2 * dir_vector), on_stairs);
            neighbor_tiles.look_ahead_tile = look_ahead_tile.tile;
            neighbor_tiles.look_ahead_other_level = ((on_tile.layer == null && look_ahead_tile.layer != null) || (on_tile.layer != null && look_ahead_tile.layer == null) ||
                (on_tile.layer != null && look_ahead_tile.layer != null && on_tile.layer.name != look_ahead_tile.layer.name));

            MatchedTile look_ahead_cw_tile = GetTileAtPosition (neighbor_maps, int_pos + cw_dir_vector, on_stairs);
            neighbor_tiles.look_ahead_cw_tile = look_ahead_cw_tile.tile;

            MatchedTile look_ahead_ccw_tile = GetTileAtPosition (neighbor_maps, int_pos + ccw_dir_vector, on_stairs);
            neighbor_tiles.look_ahead_ccw_tile = look_ahead_ccw_tile.tile;
            
            return neighbor_tiles;
        }

        private MatchedTile GetTileAtPosition (NeighborTilemaps neighbor_maps, Vector3Int pos,
                                                bool on_stairs = false, bool looking_above = false, bool looking_below = false)
        {
            MatchedTile matched_tile = new MatchedTile{};
            matched_tile.object_match = false;
            
            // Check Layer Up
            if (!looking_below && neighbor_maps.layer_up != null)
                matched_tile = CheckTilePositionOnLayer(matched_tile, pos, neighbor_maps.layer_up, neighbor_maps.objects_up, !looking_above);
            
            // Check Current Layer
            if (matched_tile.tile == null && !looking_above && !looking_below)
                matched_tile = CheckTilePositionOnLayer(matched_tile, pos, neighbor_maps.ground, neighbor_maps.objects);

            // Check Layer Down in special cases
            if ((looking_below || (on_stairs && matched_tile.tile == null)) && neighbor_maps.layer_down != null)
                matched_tile = CheckTilePositionOnLayer(matched_tile, pos, neighbor_maps.layer_down, neighbor_maps.objects_down);
            
            // Handle terrain tiles
            if (matched_tile.tile != null && (!matched_tile.object_match || 
            (matched_tile.map.tag == Constants.GROUND_LAYER_TAG && !ParallaxTerrain.IsStairTile(matched_tile.tile))))
            {
                // Extend double tall terrain front edges
                MatchedTile up_tile = new MatchedTile{};
                if (looking_below) up_tile = CheckTilePositionOnLayer (up_tile, pos + Vector3Int.up, neighbor_maps.ground, neighbor_maps.objects, true);
                else up_tile = CheckTilePositionOnLayer (up_tile, pos + Vector3Int.up, neighbor_maps.layer_up, neighbor_maps.objects_up, true);
                RuleTile up_ruletile = up_tile.tile as RuleTile;
                if (up_ruletile != null && up_ruletile.is_terrain && up_ruletile.is_double_tall)
                {
                    matched_tile.tile = up_ruletile;
                    matched_tile.map = up_tile.map;
                }
                
                RuleTile ruletile = matched_tile.tile as RuleTile;
                if (ruletile != null)
                {
                    // Detect proper surface / edge
                    bool looking_up = (neighbor_maps.layer_up != null && neighbor_maps.layer_up.name == matched_tile.map.name);
                    if (!looking_up && neighbor_maps.objects_up != null)
                    {
                        foreach (Tilemap obj_map in neighbor_maps.objects_up)
                        {
                            if (obj_map.name == matched_tile.map.name)
                                looking_up = true;
                        }
                    }
                    matched_tile.tile = ConvertTerrainRuleTile(ruletile, matched_tile.map, pos, looking_up);
                    
                    // Handle up stairs
                    matched_tile = StairTileHelper(matched_tile, ruletile, pos + Vector3Int.down, neighbor_maps.ground, neighbor_maps.objects);
                    matched_tile = StairTileHelper(matched_tile, ruletile, pos + Vector3Int.down, neighbor_maps.layer_up, neighbor_maps.objects_up);
                    matched_tile = StairTileHelper(matched_tile, ruletile, pos + Vector3Int.down + Vector3Int.left, neighbor_maps.ground, neighbor_maps.objects);
                    matched_tile = StairTileHelper(matched_tile, ruletile, pos + Vector3Int.down + Vector3Int.right, neighbor_maps.ground, neighbor_maps.objects);
                }
            }
            return matched_tile;
        }

        private MatchedTile CheckTilePositionOnLayer (MatchedTile matched_tile, Vector3Int pos, Tilemap layer, Tilemap[] objects, bool up = false)
        {
            GameObject go = null;
            if (layer != null)
                go = layer.GetInstantiatedObject(pos);
            
            // Check Object Layers Top to Bottom
            ParallaxTileBase checked_tile;
            if (matched_tile.tile == null && objects != null)
            {
                for (int i = objects.Length - 1; i >= 0; i--)
                {
                    if ((matched_tile.tile && !up))
                        break;

                    checked_tile = (ParallaxTileBase)objects[i].GetTile(pos);
                    
                    if (checked_tile && checked_tile.terrain_tag != TerrainTags.None)
                    {
                        matched_tile.tile = checked_tile;
                        matched_tile.map = objects[i];
                        matched_tile.layer = layer;
                        matched_tile.object_match = true;
                    }

                    // Ignore bridges and water on layer above, see the ground on current level instead
                    if (up && matched_tile.tile && (ParallaxTerrain.IsBridgeTile(matched_tile.tile) || ParallaxTerrain.IsWaterTile(matched_tile.tile)))
                    {
                        matched_tile.tile = null;
                        matched_tile.map = null;
                        matched_tile.layer = null;
                        matched_tile.object_match = false;
                    }
                }
            }

            // Check Base Terrain Layer
            if (layer != null)
            {
                if (matched_tile.tile == null || (!up && ParallaxTerrain.IsStairTile(matched_tile.tile) && go != null && go.tag == Constants.TERRAIN_CORNER_EDGE_TILE_TAG) || 
                (up && go != null && (go.tag == Constants.TERRAIN_EDGE_TILE_TAG || (go.tag == Constants.TERRAIN_CORNER_EDGE_TILE_TAG && !ParallaxTerrain.IsStairTile(matched_tile.tile)))) ||
                (up && matched_tile.object_match && !ParallaxTerrain.IsStairTile(matched_tile.tile) && !ParallaxTerrain.IsBridgeTile(matched_tile.tile) && !ParallaxTerrain.IsWaterTile(matched_tile.tile)))
                {
                    checked_tile = (ParallaxTileBase)layer.GetTile(pos);
                    if (checked_tile && checked_tile.terrain_tag != TerrainTags.None)
                    {
                        matched_tile.tile = checked_tile;
                        matched_tile.map = layer;
                        matched_tile.layer = layer;
                        matched_tile.object_match = false;
                    }
                }

                // Ignore Cliff Edges on layer above, see the ground on current level instead
                if (up && go != null && matched_tile.tile && ParallaxTerrain.IsTerrainTile(matched_tile.tile) && 
                    (go.tag == Constants.TERRAIN_EDGE_TILE_TAG))
                {
                    matched_tile.tile = null;
                    matched_tile.map = null;
                    matched_tile.layer = null;
                    matched_tile.object_match = false;
                }
            }

            return matched_tile;
        }

        private ParallaxTileBase ConvertTerrainRuleTile(RuleTile ruletile, Tilemap matched_map, Vector3Int pos, bool looking_up)
        {
            if (!ruletile.is_terrain) return ruletile;
            string tile_name = ruletile.name;
            ParallaxTileBase check_tile;

            if (looking_up) return ruletile;

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

        private MatchedTile StairTileHelper(MatchedTile matched_tile, RuleTile ruletile, Vector3Int offset_pos, Tilemap layer, Tilemap[] objects)
        {
            MatchedTile down_tile = new MatchedTile{};
            down_tile = CheckTilePositionOnLayer(down_tile, offset_pos, layer, objects);
            if (down_tile.tile != null && down_tile.tile.terrain_tag == TerrainTags.StairUp && ruletile.surface_tile != null)
                matched_tile.tile = ruletile.surface_tile;

            return matched_tile;
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