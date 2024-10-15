using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;
using Utilities;
using Eventing;
using Mapping.Tiles;
using Sherbert.Framework.Generic;

namespace Mapping
{
    [Serializable]
    public class Map : SerializedMonoBehaviour
    {
        #region Global

        public static SerializableDictionary<Maps, Map> map_dict;

        #endregion
        

        #region Fields

        [Title("Basic Information")]
        public Maps map_id;
        [ValidateInput("NameMatchesGameObject", "Map's name must match the game object's name.")]
        public string map_name;
        public MapRegions map_region;
        public MapType map_type;

        [Title("Layer Information")]
        public SerializableDictionary<int, Tilemap> map_layers;
        [ReadOnly]
        public Dictionary<int, Tilemap[]> object_layers;
        [ShowInInspector]
        private MapCache map_cache;

        [Title("Effect Prefabs")]
        public bool show_effect_settings = false;
        [HideIf("@!show_effect_settings")]
        public GameObject grass_rustle;
        [HideIf("@!show_effect_settings")]
        public GameObject water_splash;
        [HideIf("@!show_effect_settings")]
        public Dictionary<Directions, GameObject> footprints = new Dictionary<Directions, GameObject>() {
            {Directions.Up, null}, {Directions.Left, null}, {Directions.Right, null}, {Directions.Down, null}
        };

        // Track Animations
        private Dictionary<TilePosition, bool> cancel_anim_kill;
        private Dictionary<TilePosition, GameObject> water_splash_anims;

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
            string layer_name = is_ground ? "Ground " + layer_id : "Objects " + layer_id + "." + object_layer_id;
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

                    // Instantiate Prefab Tiles & Expand Multi Tiles
                    foreach (Vector3Int pos in object_layer.cellBounds.allPositionsWithin)
                    {
                        Vector3Int local_place = new Vector3Int(pos.x, pos.y, pos.z);

                        if (object_layer.HasTile(local_place))
                        {
                            ParallaxTileBase tile = object_layer.GetTile<ParallaxTileBase>(local_place);
                            BasicTile basic_tile = tile as BasicTile;
                            
                            // Expand Basic Multi Tiles
                            if (basic_tile && basic_tile.multi_tile)
                            {
                                basic_tile.ExpandTile(local_place, object_layer);
                                continue;
                            }

                            // Instantiate & Expand Prefab Tiles
                            PrefabTile prefab_tile = tile as PrefabTile; 
                            if (prefab_tile)
                            {
                                prefab_tile.InstantiatePrefab(local_place, object_layer);
                                continue;
                            }
                        }
                    }
                    
                    // Dynamic set sorting layers
                    TilemapRenderer renderer = object_layer.GetComponent<TilemapRenderer>();
                    renderer.sortingOrder = sorting_layer + i + Constants.OBJECT_LAYER_START_OFFSET;                        
                }
            }

            // Setup map cache if not already done
            if (map_cache == null) map_cache = MapCache.Create(this);

            // Initialize animation tracking dictionaries
            cancel_anim_kill = new Dictionary<TilePosition, bool>();
            water_splash_anims = new Dictionary<TilePosition, GameObject>(); 
        }
        
        #endregion


        #region Map Effects

        /*
        * Checks if pos is below a bridge, returns true if so, false if not. 
        * If position is below a bridge, hides that bridge. If not, shows the previously hidden bridge.
        */
        public bool HideBridgeAbovePosition(Vector3 pos)
        {
            if (!Settings.SEE_THROUGH_BRIDGES) return false;

            NeighborTilemaps neighbor_maps = GetNeighborTileMaps(pos);
            Vector3Int int_pos = new Vector3Int (
                (int)(pos.x),
                (int)(pos.y),
                0
            );

            MatchedTile above_tile = new MatchedTile{};
            above_tile = CheckTilePositionOnLayer (above_tile, int_pos, neighbor_maps.layer_up, neighbor_maps.objects_up, true, true);
            
            // Detect bridges over cliff edges
            if (above_tile.tile == null && neighbor_maps.layer_up != null)
            {
                GameObject go = neighbor_maps.layer_up.GetInstantiatedObject(int_pos);
                if (go != null && (go.tag == Constants.TERRAIN_EDGE_TILE_TAG || go.tag == Constants.TERRAIN_CORNER_EDGE_TILE_TAG || go.tag == Constants.TERRAIN_EDGE_CORNER_TILE_TAG))
                    above_tile = CheckTilePositionOnLayer (above_tile, int_pos + Vector3Int.up, neighbor_maps.layer_up, neighbor_maps.objects_up, true, true);
            }
            

            if (above_tile.tile != null && ParallaxTerrain.IsBridgeTile(above_tile.tile))
            {
                // Hide Bridge
                if (!map_cache.bridge_hidden)
                {
                    // See if this bridge was previously hidden
                    List<TilePosition> hidden_bridge = map_cache.GetBridge(new TilePosition(above_tile.map, int_pos));
                    if (hidden_bridge != null)
                    {
                        // Bridge already found / saved
                        // Debug.Log("Bridge already saved!");
                    }
                    // If not, find the bridge
                    else
                    {
                        // Get all connected bridge tiles
                        List<TilePosition> bridge_tiles = GetMatchingConnectedTiles(above_tile.map, int_pos, above_tile.tile);
                        
                        // Get all objects above them
                        List<TilePosition> object_tiles = GetObjectsOnLayerAtPositions(neighbor_maps.objects_up, bridge_tiles);
                        bridge_tiles.AddRange(object_tiles);

                        hidden_bridge = bridge_tiles;
                    }

                    // Make all tiles transparent
                    map_cache.bridge_hidden = true;
                    map_cache.cur_bridge = hidden_bridge;
                    map_cache.CacheBridge(hidden_bridge);
                    HideTiles(map_cache.cur_bridge);
                }

                return true;
            }
            else 
            {
                // Show Bridge Again
                if (map_cache.bridge_hidden)
                {
                    // Make all tiles opaque
                    ShowTiles(map_cache.cur_bridge);
                    map_cache.bridge_hidden = false;
                }

                return false;
            }
        }

        /*
        * Checks if a position is hidden by a higher terrain layer, returns true if so, false if not.
        * If the position is hidden by a terrain layer, hides that terrain layer and all above it. If not, shows the previously hidden tiles.
        */
        public bool HideLayersAbovePosition(Vector3 pos, int start_n_up = 2)
        {
            if (!Settings.SEE_THROUGH_TERRAIN) return false;
            
            // Get layer that is start_n_up from current layer
            pos -= new Vector3(0, 0, (start_n_up * Constants.MAP_LAYER_HEIGHT));
            int start_layer_id = GetMapLayerIDFromPosition(pos);
            if (start_layer_id == int.MinValue) return false;

            // Find tile directly above player at that layer
            Vector3Int int_pos = new Vector3Int (
                (int)(pos.x),
                (int)(pos.y),
                0
            );
            MatchedTile start_tile = new MatchedTile{};
            start_tile = CheckTilePositionOnLayer (start_tile, int_pos, map_layers[start_layer_id], object_layers[start_layer_id], true, false, true);
            bool behind_terrain = (start_tile.tile != null);

            // Check higher layers to see if behind any of them
            if (!behind_terrain)
            {
                foreach (KeyValuePair<int, Tilemap> layer in map_layers)
                {
                    if (layer.Key < start_layer_id + 1) continue;
                    start_tile = CheckTilePositionOnLayer(start_tile, int_pos, layer.Value, object_layers[layer.Key], true, false, true);

                    if (start_tile.tile != null)
                    {
                        behind_terrain = true;
                        start_layer_id = layer.Key;
                        break;
                    }
                }
            }

            if (behind_terrain) // tile on base layer
            {
                // Hide Terrain
                bool needs_update = true;
                if (map_cache.terrain_hidden && map_cache.cur_terrain.Contains(new TilePosition (start_tile.map, int_pos)))
                {
                    needs_update = false;
                    // Debug.Log("No update needed.");
                }

                if (needs_update)
                {
                    // See if this terrain was previously hidden
                    List<TilePosition> hidden_layers = map_cache.GetTerrain(new TilePosition (start_tile.map, int_pos));
                    if (hidden_layers != null)
                    {
                        // Terrain already found / saved
                        // Debug.Log("Terrain already saved!");
                    }
                    // If not, find the terrain
                    else
                    {
                        hidden_layers = new List<TilePosition>();
                        List<TilePosition> terrain_tiles = null; 
                        ParallaxTileBase init_tile = null;
                        bool first_loop = true;

                        foreach (KeyValuePair<int, Tilemap> layer in map_layers)
                        {
                            if (layer.Key < start_layer_id) continue;

                            // Check the next layer map
                            int layer_diff = layer.Key - start_layer_id;
                            if (first_loop) 
                            {
                                init_tile = start_tile.tile;
                                first_loop = false;
                            }
                            else
                            {
                                int_pos = new Vector3Int((int)pos.x, (int)pos.y + layer_diff, 0);
                                start_tile = new MatchedTile{};
                                start_tile = CheckTilePositionOnLayer(start_tile, int_pos, layer.Value, object_layers[layer.Key], true, false, true);
                            }

                            // Get all connected terrain tiles
                            terrain_tiles = GetMatchingConnectedTiles(layer.Value, int_pos, init_tile, layer_diff + 1, terrain_tiles);
                            
                            // Get all objects above them
                            List<TilePosition> object_tiles = GetObjectsOnLayerAtPositions(object_layers[layer.Key], terrain_tiles);
                            terrain_tiles.AddRange(object_tiles);

                            // Add to the master list
                            hidden_layers.AddRange(terrain_tiles);
                        }
                    }

                    // Make all tiles transparent
                    map_cache.terrain_hidden = true;
                    map_cache.cur_terrain = hidden_layers;
                    map_cache.CacheTerrain(hidden_layers);
                    HideTiles(map_cache.cur_terrain);
                }
                return true;
            }
            else 
            {
                // Show Terrain Again
                if (map_cache.terrain_hidden)
                {
                    // Make all tiles opaque
                    ShowTiles(map_cache.cur_terrain);
                    map_cache.terrain_hidden = false;
                }
                return false;
            }
        }

        /*
        * Checks if a position is hidden by a prefab, returns true if so, false if not.
        * If hidden by a hideable prefab, hides that prefab. If not, shows previously hidden prefab.
        */
        public bool HidePrefabBlockingPosition(Vector3 pos)
        {
            if (!Settings.SEE_THROUGH_PREFAB_OBJECTS) return false;

            NeighborTilemaps neighbor_maps = GetNeighborTileMaps(pos);
            Vector3Int int_pos = new Vector3Int (
                (int)(pos.x),
                (int)(pos.y) - 1,
                0
            );

            // Get prefab
            MatchedTile down_tile = new MatchedTile{};
            down_tile = CheckTilePositionOnLayer (down_tile, int_pos + Vector3Int.up, neighbor_maps.layer_up, neighbor_maps.objects_up);
            if (down_tile.tile == null)
                down_tile = CheckTilePositionOnLayer (down_tile, int_pos, neighbor_maps.layer_up, neighbor_maps.objects_up);
            if (down_tile.tile == null) 
                down_tile = CheckTilePositionOnLayer (down_tile, int_pos, neighbor_maps.ground, neighbor_maps.objects);
            GameObject prefab_obj = null;
            if (down_tile.tile != null)
                prefab_obj = down_tile.tile.instantiated_object;
            
            bool should_hide = false;
            if (prefab_obj != null && down_tile.tile.is_hideable)
            {
                should_hide = true;
                if (down_tile.tile.strict_hiding)
                {
                    MatchedTile up_tile = new MatchedTile{};
                    up_tile = CheckTilePositionOnLayer (up_tile, int_pos + Vector3Int.up, neighbor_maps.layer_up, neighbor_maps.objects_up);
                    if (up_tile.tile == null) 
                        up_tile = CheckTilePositionOnLayer (up_tile, int_pos + Vector3Int.up, neighbor_maps.ground, neighbor_maps.objects);
                    
                    if (up_tile.tile != null)
                    {
                        GameObject up_prefab_obj = up_tile.tile.instantiated_object;
                        if (up_prefab_obj == prefab_obj)
                            should_hide = true;
                        else should_hide = false;
                    }
                    else should_hide = false;
                }
            }
            if (should_hide)
            {
                // Hide Prefab
                bool needs_update = true;
                if (map_cache.prefab_hidden)
                    needs_update = (map_cache.cur_prefab != prefab_obj);

                if (needs_update)
                {
                    // Show the previous prefab
                    if (map_cache.prefab_hidden)
                        ShowTiles(null, map_cache.cur_prefab);
                    
                    // Make all tiles transparent
                    HideTiles(null, prefab_obj);
                    map_cache.cur_prefab = prefab_obj;
                }
                map_cache.prefab_hidden = true;
                return true;
            }
            else
            {
                // Show Prefab Again
                if (map_cache.prefab_hidden)
                {
                    // Make prefab opaque
                    ShowTiles(null, map_cache.cur_prefab);
                    map_cache.prefab_hidden = false;
                }
                return false;
            }
        }

        /*
        * Makes all tiles in a list of tile positions transparent / hidden
        */
        private void HideTiles (List<TilePosition> tiles, GameObject prefab_obj = null)
        {
            if (prefab_obj != null)
            {
                foreach (SpriteRenderer sprite in prefab_obj.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (sprite.tag == Constants.UP_LAYER_PRIORITY_TILE_TAG)
                        sprite.color = new Color(1,1,1,Constants.HIDDEN_LAYER_TILE_ALPHA);
                }
                return;
            }
            
            foreach (TilePosition tile in tiles)
            {
                // See if tile directly below is hidden
                bool hide_entirely = false;
                if (!tile.is_object)
                {
                    int layer_below = GetMapLayerIDFromPosition(tile.map.transform.position + new Vector3(0, 0, Constants.MAP_LAYER_HEIGHT));
                    if (layer_below != int.MinValue)
                    {
                        TilePosition tile_below = new TilePosition(map_layers[layer_below], tile.pos);
                        hide_entirely = tiles.Contains(tile_below);
                    }
                }

                float hide_alpha = hide_entirely ? 0 : Constants.HIDDEN_LAYER_TILE_ALPHA;
                
                // Hide basic tiles
                tile.map.SetColor(tile.pos, new Color(1,1,1,hide_alpha));

                // Hide prefabs / game objects
                GameObject go = null;
                go = GetGameObjectOnLayer(go, tile.pos, null, null, tile.map);
                if (go != null)
                foreach (SpriteRenderer sprite in go.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (sprite.tag == Constants.DOWN_LAYER_PRIORITY_TILE_TAG)
                        sprite.color = new Color (1,1,1,0); // Hide "gap fill" tiles entirely
                    else if ((go.tag == Constants.TERRAIN_EDGE_TILE_TAG || go.tag == Constants.TERRAIN_EDGE_CORNER_TILE_TAG) && (sprite.tag == Constants.EXTRA_DEPRIORITY_TILE_TAG || sprite.tag == Constants.DEPRIORITY_TILE_TAG))
                        sprite.color = new Color (1,1,1,0); // Hide "back edge" faces entirely
                    else
                        sprite.color = new Color(1,1,1,hide_alpha);
                }
            }
        }

        /*
        * Makes all tiles in a list of tile positions opaque / visible
        */
        private void ShowTiles (List<TilePosition> tiles, GameObject prefab_obj = null)
        {
            if (prefab_obj != null)
            {
                foreach (SpriteRenderer sprite in prefab_obj.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (sprite.tag == Constants.UP_LAYER_PRIORITY_TILE_TAG)
                        sprite.color = new Color(1,1,1,1);
                }
                return;
            }

            foreach (TilePosition tile in tiles)
            {
                // Show basic tiles
                tile.map.SetColor(tile.pos, new Color(1,1,1,1));

                // Show prefabs
                GameObject go = null;
                go = GetGameObjectOnLayer(go, tile.pos, null, null, tile.map);
                if (go != null)
                foreach (SpriteRenderer sprite in go.GetComponentsInChildren<SpriteRenderer>())
                    sprite.color = new Color(1,1,1,1);
            }
        }
        
        /*
        * Play tall grass activated animation
        */
        public IEnumerator GrassRustleAnimation(Vector3 pos)
        {
            int layer = GetMapLayerIDFromPosition(pos);
            Tilemap map = map_layers[layer];

            GameObject anim = Instantiate(grass_rustle, map.transform);
            anim.transform.position = pos;

            SpriteUtils.ConfigurePrefabTileSprites(map, anim, false, false, true); // Behind player

            yield return new WaitForSeconds(0.5f);

            GameObject.Destroy(anim.gameObject);
        }

        /* 
        * Play footprints animation
        */
        public IEnumerator FootprintsAnimation(Vector3 pos, Directions dir, bool sneaking)
        {
            int layer = GetMapLayerIDFromPosition(pos);
            Tilemap map = map_layers[layer];

            if (sneaking) yield return new WaitForSeconds(0.4f);
            else yield return new WaitForSeconds(0.2f);
            
            GameObject anim = Instantiate(footprints[dir], map.transform);
            anim.transform.position = pos;
            SpriteUtils.ConfigurePrefabTileSprites(map, anim, false, true, true); // Behind player

            yield return new WaitForSeconds(0.5f);
            
            SpriteRenderer sprite = anim.GetComponentInChildren<SpriteRenderer>();
            Color start = new Color(1,1,1,Constants.LIGHT_TRANS_TILE_ALPHA);
            Color end = new Color (1,1,1,0);
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(0.1f);
                sprite.color = Color.Lerp(start, end, (i / 5f));
            }

            GameObject.Destroy(anim.gameObject);
        }

        /* 
        * Start water splash animation
        */
        public IEnumerator WaterSplashAnimation(Vector3 pos)
        {
            int layer = GetMapLayerIDFromPosition(pos);
            Tilemap map = map_layers[layer];          

            yield return new WaitForSeconds(water_splash.GetComponent<MapAnimationSettings>().appear_delay);

            TilePosition splash_pos = new TilePosition(map, new Vector3Int((int)pos.x, (int)pos.y));
            if (!water_splash_anims.ContainsKey(splash_pos))
            {
                GameObject anim = Instantiate(water_splash, map.transform);
                anim.transform.position = pos;
                SpriteUtils.ConfigurePrefabTileSprites(map, anim, false, false, false, true); // Same layer as player
                water_splash_anims.Add(splash_pos, anim);
            }
            else
            {
                if (!cancel_anim_kill.ContainsKey(splash_pos))
                    cancel_anim_kill.Add(splash_pos,true);
            }
        }

        /* 
        * Kill water splash animation
        */
        public IEnumerator KillWaterSplashAnimation(Vector3 pos)
        {
            int layer = GetMapLayerIDFromPosition(pos);
            Tilemap map = map_layers[layer];

            yield return new WaitForSeconds(water_splash.GetComponent<MapAnimationSettings>().kill_delay);
            TilePosition splash_pos = new TilePosition(map, new Vector3Int((int)pos.x, (int)pos.y));

            if (cancel_anim_kill.ContainsKey(splash_pos))
                cancel_anim_kill.Remove(splash_pos);
            else if (water_splash_anims.ContainsKey(splash_pos))
            {
                GameObject splash = water_splash_anims[splash_pos];
                water_splash_anims.Remove(splash_pos);
                GameObject.Destroy(splash.gameObject);
            }
        }

        /*
        * Sets proper mask sprites for reflections. reflection_masks is an array in the following order:
        * on_tile, left_tile, right_tile, down_tile, down_left_tile, down_right_tile, down_two_tile, down_two_left_tile, down_two_right_tile
        */
        public void SetReflectionMask(Vector3 pos, SpriteMask[] masks)
        {
            int layer_id = GetMapLayerIDFromPosition(pos);
            Tilemap layer = map_layers[layer_id];
            Tilemap[] object_layer = object_layers[layer_id];
            Vector3Int int_pos = new Vector3Int((int)pos.x, (int)pos.y, 0);

            Sprite[] sprites = new Sprite[masks.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                masks[i].sprite = null;
                sprites[i] = null;
            }

            Vector3Int[] positions = new Vector3Int[masks.Length];
            positions[0] = int_pos;
            positions[1] = int_pos + Vector3Int.left;
            positions[2] = int_pos + Vector3Int.right;
            positions[3] = int_pos + Vector3Int.down;
            positions[4] = int_pos + Vector3Int.down + Vector3Int.left;
            positions[5] = int_pos + Vector3Int.down + Vector3Int.right;
            positions[6] = int_pos + Vector3Int.down + Vector3Int.down;
            positions[7] = int_pos + Vector3Int.down + Vector3Int.down + Vector3Int.left;
            positions[8] = int_pos + Vector3Int.down + Vector3Int.down + Vector3Int.right;

            // Work from bottom layer up for each position, taking the lowest reflective tile
            Tilemap[] layer_maps = new Tilemap[1 + object_layer.Length];
            layer_maps[0] = layer;
            for (int i = 0; i < object_layer.Length; i++)
                layer_maps[i + 1] = object_layer[i];

            foreach (Tilemap map in layer_maps)
            {
                for (int i = 0; i < masks.Length; i++)
                {
                    Vector3Int tile_pos = positions[i];
                    ParallaxTileBase tile = map.GetTile<ParallaxTileBase>(tile_pos);
                    RuleTile ruletile = tile as RuleTile;
                    GameObject go = map.GetInstantiatedObject(tile_pos);
                    
                    if (sprites[i] == null || go != null)
                    {
                        if (tile != null && tile.is_reflective)
                        {
                            if (go != null)
                            {
                                SpriteRenderer renderer = go.GetComponentInChildren<SpriteRenderer>();
                                sprites[i] = renderer.sprite;
                            }
                            else sprites[i] = map.GetSprite(tile_pos);
                        }
                    }
                }
            }

            // Set masks
            for (int i = 0; i < sprites.Length; i++)
                masks[i].sprite = sprites[i];
        }
        
        #endregion


        #region Map Awareness
        
        public int GetMapLayerIDFromPosition (Vector3 pos)
        {
            foreach (KeyValuePair<int, Tilemap> layer in map_layers)
            {
                Tilemap map = layer.Value;
                if (map.transform.position.z == pos.z) return layer.Key;
            }

            return int.MinValue;
        }
        
        private NeighborTilemaps GetNeighborTileMaps (Vector3 pos)
        {
            NeighborTilemaps neighbor_maps = new NeighborTilemaps{};

            foreach (KeyValuePair<int, Tilemap> layer in map_layers)
            {
                Tilemap map = layer.Value;
                if (map.transform.position.z == pos.z)
                {
                    neighbor_maps.ground_layer_id = layer.Key;
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

        public GameObject GetGameObjectAtPosition (Vector3 pos)
        {
            NeighborTilemaps neighbor_maps = GetNeighborTileMaps(pos);
            Vector3Int int_pos = new Vector3Int (
                (int)(pos.x),
                (int)(pos.y),
                0
            );

            GameObject found_object = null;
            found_object = GetGameObjectOnLayer (found_object, int_pos, neighbor_maps.layer_up, neighbor_maps.objects_up);
            if (!found_object) found_object = GetGameObjectOnLayer (found_object, int_pos, neighbor_maps.ground, neighbor_maps.objects);

            return found_object;
        }

        private GameObject GetGameObjectOnLayer (GameObject go, Vector3Int pos, Tilemap layer, Tilemap[] objects, Tilemap map = null)
        {
            ParallaxTileBase tile = null;

            if (map != null)
            {
                tile = (ParallaxTileBase)map.GetTile(pos);
                PrefabTile prefab_tile = tile as PrefabTile;
                if (prefab_tile) go = prefab_tile.GetInstantiatedObject();
                else go = map.GetInstantiatedObject(pos);
            }
            else 
            {
                if (go == null && objects != null)
                {
                    for (int i = objects.Length - 1; i >= 0; i--)
                    {
                        if (go != null) break;
                        
                        tile = (ParallaxTileBase)objects[i].GetTile(pos);
                        PrefabTile prefab_tile = tile as PrefabTile;
                        if (prefab_tile) go = prefab_tile.prefab;
                        else go = objects[i].GetInstantiatedObject(pos);
                    }
                }
                if (go == null && layer != null)
                    go = layer.GetInstantiatedObject(pos);
            }

            return go;
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
            bool underwater = false;

            Vector3Int int_pos = new Vector3Int (
                (int)(pos.x),
                (int)(pos.y),
                0
            );

            // Current Tile - modifies some behavior of neighbor tile detection/perception
            MatchedTile on_tile = GetTileAtPosition (neighbor_maps, int_pos, false, false, false, true);
            neighbor_tiles.on_tile = on_tile.tile;

            if (!look_only)
            {
                if (neighbor_tiles.on_tile != null)
                {
                    on_stairs = (neighbor_tiles.on_tile != null && ParallaxTerrain.IsStairTile(neighbor_tiles.on_tile));
                    on_water = (neighbor_tiles.on_tile != null && ParallaxTerrain.IsWaterTile(neighbor_tiles.on_tile));
                    on_bridge = (neighbor_tiles.on_tile != null && ParallaxTerrain.IsBridgeTile(neighbor_tiles.on_tile));
                    underwater = (neighbor_tiles.on_tile != null && ParallaxTerrain.IsUnderwaterTile(neighbor_tiles.on_tile));
                }
            
                // Same Level Neighbors
                neighbor_tiles.up_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.up, on_stairs, false, false, underwater).tile;
                neighbor_tiles.left_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.left, on_stairs, false, false, underwater).tile;
                neighbor_tiles.right_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.right, on_stairs, false, false, underwater).tile;
                neighbor_tiles.down_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down, on_stairs, false, false, underwater).tile;

                // Above / Below Level Neighbors - primarily bridge & water use
                neighbor_tiles.above_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.up, on_stairs, true, false, underwater).tile;
                if (on_bridge || on_water)
                    neighbor_tiles.below_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down, on_stairs, false, true, underwater).tile;

                // Corner Neighbors
                neighbor_tiles.up_left_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.up + Vector3Int.left, on_stairs, false, false, underwater).tile;
                neighbor_tiles.up_right_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.up + Vector3Int.right, on_stairs, false, false, underwater).tile;
                neighbor_tiles.down_left_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down + Vector3Int.left, on_stairs, false, false, underwater).tile;
                neighbor_tiles.down_right_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down + Vector3Int.right, on_stairs, false, false, underwater).tile;
                
                // Extra Special Case Needs
                neighbor_tiles.down_two_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down + Vector3Int.down, on_stairs, false, false, underwater).tile;
                neighbor_tiles.down_two_left_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down + Vector3Int.down + Vector3Int.left, on_stairs, false, false, underwater).tile;
                neighbor_tiles.down_two_right_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down + Vector3Int.down + Vector3Int.right, on_stairs, false, false, underwater).tile;
                neighbor_tiles.down_left_two_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down +Vector3Int.left + Vector3Int.left, on_stairs, false, false, underwater).tile;
                neighbor_tiles.down_right_two_tile = GetTileAtPosition (neighbor_maps, int_pos + Vector3Int.down +Vector3Int.right + Vector3Int.right, on_stairs, false, false, underwater).tile;
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

            MatchedTile facing_tile = GetTileAtPosition (neighbor_maps, int_pos + dir_vector, on_stairs, false, false, underwater, false);
            neighbor_tiles.facing_tile = facing_tile.tile;
            neighbor_tiles.facing_other_level = ((on_tile.layer == null && facing_tile.layer != null) || (on_tile.layer != null && facing_tile.layer == null) ||
                (on_tile.layer != null && facing_tile.layer != null && on_tile.layer.name != facing_tile.layer.name));

            MatchedTile look_ahead_tile = GetTileAtPosition (neighbor_maps, int_pos + (2 * dir_vector), on_stairs, false, false, underwater, false);
            neighbor_tiles.look_ahead_tile = look_ahead_tile.tile;
            neighbor_tiles.look_ahead_other_level = ((on_tile.layer == null && look_ahead_tile.layer != null) || (on_tile.layer != null && look_ahead_tile.layer == null) ||
                (on_tile.layer != null && look_ahead_tile.layer != null && on_tile.layer.name != look_ahead_tile.layer.name));

            MatchedTile look_ahead_cw_tile = GetTileAtPosition (neighbor_maps, int_pos + cw_dir_vector, on_stairs, false, false, underwater, false);
            neighbor_tiles.look_ahead_cw_tile = look_ahead_cw_tile.tile;

            MatchedTile look_ahead_ccw_tile = GetTileAtPosition (neighbor_maps, int_pos + ccw_dir_vector, on_stairs, false, false, underwater, false);
            neighbor_tiles.look_ahead_ccw_tile = look_ahead_ccw_tile.tile;
            
            return neighbor_tiles;
        }

        private MatchedTile GetTileAtPosition (NeighborTilemaps neighbor_maps, Vector3Int pos,
                                                bool on_stairs = false, bool looking_above = false, bool looking_below = false,
                                                bool underwater = false, bool cache_result = true)
        {
            // Check Cache First
            int cache_layer = neighbor_maps.ground_layer_id + (1 * looking_above.ToInt()) - (1 * looking_below.ToInt());
            MatchedTile matched_tile = new MatchedTile{};
            matched_tile = map_cache.GetTile(cache_layer, pos);
            if (matched_tile.tile != null) return matched_tile;
            
            // Otherwise, find this tile
            matched_tile.object_match = false;
            
            // Check Layer Up
            if (!looking_below && neighbor_maps.layer_up != null)
                matched_tile = CheckTilePositionOnLayer(matched_tile, pos, neighbor_maps.layer_up, neighbor_maps.objects_up, !looking_above, false, false, underwater);
            
            // Check Current Layer
            if (matched_tile.tile == null && !looking_above && !looking_below)
                matched_tile = CheckTilePositionOnLayer(matched_tile, pos, neighbor_maps.ground, neighbor_maps.objects);

            // Check Layer Down in special cases
            if ((looking_below || (on_stairs && matched_tile.tile == null)) && neighbor_maps.layer_down != null)
                matched_tile = CheckTilePositionOnLayer(matched_tile, pos, neighbor_maps.layer_down, neighbor_maps.objects_down);
            
            // Handle terrain tiles
            if (matched_tile.tile != null && (!matched_tile.object_match || 
            (!ParallaxTerrain.IsStairTile(matched_tile.tile) && !ParallaxTerrain.IsWaterTile(matched_tile.tile))))
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
            // Handle water / shore tiles
            if (!looking_above && matched_tile.tile != null && ParallaxTerrain.IsWaterTile(matched_tile.tile, false, true))
            {
                ParallaxTileBase terrain_tile = (ParallaxTileBase)neighbor_maps.ground.GetTile(pos);
                RuleTile ruletile = matched_tile.tile as RuleTile;

                GameObject go = neighbor_maps.ground.GetInstantiatedObject(pos);
                
                // Shore Flat Surface Detection
                if (ruletile != null && ruletile.shore_tile != null && terrain_tile != null &&
                    (go == null || go.tag == Constants.TERRAIN_EDGE_TILE_TAG || go.tag == Constants.TERRAIN_CORNER_EDGE_TILE_TAG))
                {
                    matched_tile.tile = ruletile.shore_tile;
                    matched_tile = CheckTilePositionOnLayer(matched_tile, pos, neighbor_maps.ground, neighbor_maps.objects,false,false,false,false,true);
                }
            }

            // Cache the newly found tile before returning it
            if (cache_result) map_cache.SetTile(cache_layer, pos, matched_tile);
            return matched_tile;
        }

        private MatchedTile CheckTilePositionOnLayer (MatchedTile matched_tile, Vector3Int pos, Tilemap layer, Tilemap[] objects,
                                                        bool up = false, bool see_bridge = false, bool ignore_objects = false, bool underwater = false, bool see_through_shore = false)
        {
            GameObject go = null;
            if (layer != null)
                go = layer.GetInstantiatedObject(pos);
            
            // Check Object Layers Top to Bottom
            ParallaxTileBase checked_tile;
            if ((matched_tile.tile == null && objects != null && !ignore_objects) || see_through_shore)
            {
                for (int i = objects.Length - 1; i >= 0; i--)
                {
                    if ((matched_tile.tile && !up && !see_bridge && !see_through_shore))
                        break;

                    checked_tile = (ParallaxTileBase)objects[i].GetTile(pos);
                    
                    if (checked_tile && checked_tile.terrain_tag != TerrainTags.None && 
                    !(see_through_shore && ParallaxTerrain.IsWaterTile(checked_tile)))
                    {
                        matched_tile.tile = checked_tile;
                        matched_tile.map = objects[i];
                        matched_tile.layer = layer;
                        matched_tile.object_match = true;
                    }

                    // Ignore bridges and water on layer above, see the ground on current level instead
                    if (up && !see_bridge && matched_tile.tile && (ParallaxTerrain.IsBridgeTile(matched_tile.tile) || ParallaxTerrain.IsWaterTile(matched_tile.tile)))
                    {
                        matched_tile.tile = null;
                        matched_tile.map = null;
                        matched_tile.layer = null;
                        matched_tile.object_match = false;
                    }
                }
            }

            // Check Base Terrain Layer
            if (layer != null && !see_through_shore)
            {
                if (matched_tile.tile == null || (!up && ParallaxTerrain.IsStairTile(matched_tile.tile) && go != null && go.tag == Constants.TERRAIN_CORNER_EDGE_TILE_TAG) || 
                (up && !see_bridge && go != null && (go.tag == Constants.TERRAIN_EDGE_TILE_TAG || go.tag == Constants.TERRAIN_EDGE_CORNER_TILE_TAG || (go.tag == Constants.TERRAIN_CORNER_EDGE_TILE_TAG && !ParallaxTerrain.IsStairTile(matched_tile.tile)))) ||
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
                if ((Settings.ALLOW_WALK_BEHIND_TERRAIN_EDGES || underwater) && up && go != null &&
                    matched_tile.tile && ParallaxTerrain.IsTerrainTile(matched_tile.tile) && 
                    (go.tag == Constants.TERRAIN_EDGE_TILE_TAG || go.tag == Constants.TERRAIN_EDGE_CORNER_TILE_TAG))
                {
                    matched_tile.tile = null;
                    matched_tile.map = null;
                    matched_tile.layer = null;
                    matched_tile.object_match = false;
                }
            }

            return matched_tile;
        }

        private ParallaxTileBase ConvertTerrainRuleTile(RuleTile ruletile, Tilemap matched_map, Vector3Int pos, bool looking_up = false)
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

        /*
        * Returns all tiles with the same terrain tag on the specified tilemap (layer) touching a specific start tile
        */
        private List<TilePosition> GetMatchingConnectedTiles(Tilemap map, Vector3Int start_pos, ParallaxTileBase start_tile,
                                                                int top_n_rows_only = 0, List<TilePosition> expanded_start = null)
        {   
            // Mark all tiles as not visited
            Dictionary<int, Dictionary<int, bool>> visited_tiles = new Dictionary<int, Dictionary<int, bool>>();
            for (int x = map.cellBounds.min.x - 1; x <= map.cellBounds.max.x; x++)
            {
                visited_tiles.Add(x, new Dictionary<int, bool>());
                for (int y = map.cellBounds.min.y - 1; y <= map.cellBounds.max.y; y++)
                {
                    visited_tiles[x].Add(y, false);
                }
            }
            List<TilePosition> connected_tiles = new List<TilePosition>();

            // Add starting tile to queue
            Queue<TilePosition> queue = new Queue<TilePosition>();
            TilePosition start = new TilePosition (map, start_pos);
            queue.Enqueue(start);

            // Add expanded start tiles if provided
            if (expanded_start != null)
            {
                foreach (TilePosition exp in expanded_start)
                {
                    TilePosition exp_offset = new TilePosition (map, exp.pos + new Vector3Int(0, 1, 0));
                    queue.Enqueue(exp_offset);
                }
            }
            
            // Floodfill Algorithm
            while (queue.Any())
            {
                // Check if tile has already been visited
                TilePosition cur_tile = queue.Dequeue();
                if (!visited_tiles.ContainsKey(cur_tile.pos.x) || !visited_tiles[cur_tile.pos.x].ContainsKey(cur_tile.pos.y) ||
                        visited_tiles[cur_tile.pos.x][cur_tile.pos.y] == true)
                    continue;

                // Mark tile as visited
                visited_tiles[cur_tile.pos.x][cur_tile.pos.y] = true;
                
                // If this is a matching tile, add it and queue up neighbors
                ParallaxTileBase tile = (ParallaxTileBase)map.GetTile(cur_tile.pos);
                bool should_skip = false;
                if (top_n_rows_only > 0)
                {
                    ParallaxTileBase check_tile = (ParallaxTileBase)map.GetTile(cur_tile.pos + (top_n_rows_only * Vector3Int.up));
                    should_skip = (cur_tile.pos != start_pos && tile != null && check_tile != null && check_tile.terrain_tag == tile.terrain_tag);
                }
                if (!should_skip && start_tile != null && tile != null && (tile.terrain_tag == start_tile.terrain_tag))
                {
                    connected_tiles.Add(cur_tile);

                    // Queue up neighbors
                    TilePosition up_tile = new TilePosition(map, cur_tile.pos + new Vector3Int(0, 1, 0));
                    queue.Enqueue(up_tile);

                    TilePosition left_tile = new TilePosition(map, cur_tile.pos + new Vector3Int(-1, 0, 0));
                    queue.Enqueue(left_tile);

                    TilePosition right_tile = new TilePosition(map, cur_tile.pos + new Vector3Int(1, 0, 0));
                    queue.Enqueue(right_tile);

                    TilePosition down_tile = new TilePosition(map, cur_tile.pos + new Vector3Int(0, -1, 0));
                    queue.Enqueue(down_tile);
                }
            }

            // Edge case to remove start tile when it shouldn't be included in hidden tiles
            if (top_n_rows_only == 1)
            {
                ParallaxTileBase check_tile = (ParallaxTileBase)map.GetTile(start.pos + (top_n_rows_only * Vector3Int.up));
                if (start_tile != null && check_tile != null && check_tile.terrain_tag == start_tile.terrain_tag)
                    connected_tiles.Remove(start);
            }
                

            return connected_tiles;
        }

        /*
        * Returns all tiles with the same terrain tag on the specified tilemap (layer) 
        */
        private List<TilePosition> GetMatchingTilesOnLayer(Tilemap map, ParallaxTileBase source_tile)
        {
            List<TilePosition> matching_tiles = new List<TilePosition>();

            for (int x = map.cellBounds.min.x; x <= map.cellBounds.max.x; x++)
            {
                for (int y = map.cellBounds.min.y; y <= map.cellBounds.max.y; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    ParallaxTileBase tile = (ParallaxTileBase)map.GetTile(pos);
                    if (tile && (tile.terrain_tag == source_tile.terrain_tag))
                        matching_tiles.Add(new TilePosition(map, pos));
                }
            }

            return matching_tiles;
        }

        private List<TilePosition> GetObjectsOnLayerAtPositions(Tilemap[] object_layers, List<TilePosition> tile_positions)
        {
            List<TilePosition> object_positions = new List<TilePosition>();

            for (int i = object_layers.Length - 1; i >= 0; i--)
            {
                Tilemap object_map = object_layers[i];
                foreach (TilePosition tile_pos in tile_positions)
                {
                    ParallaxTileBase tile = (ParallaxTileBase)object_map.GetTile(tile_pos.pos);
                    if (tile != null)
                        object_positions.Add(new TilePosition(object_map, tile_pos.pos, true));
                        
                    // Get stairs too!
                    ParallaxTileBase below_tile = (ParallaxTileBase)object_map.GetTile(tile_pos.pos + Vector3Int.down);
                    if (below_tile != null && ParallaxTerrain.IsStairTile(below_tile, false, true))
                        object_positions.Add(new TilePosition(object_map, tile_pos.pos + Vector3Int.down, true));
                }
            }

            return object_positions;
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