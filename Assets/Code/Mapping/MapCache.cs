using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sherbert.Framework.Generic;
using Mapping.Tiles;

namespace Mapping
{
    #region Data Classes

    [Serializable]
    public class MapLayer
    {
        public int layer_id;
        public SerializableDictionary<Vector2Int, MatchedTile> tiles;

        public MapLayer(int layer_id)
        {
            this.layer_id = layer_id;
            this.tiles = new SerializableDictionary<Vector2Int, MatchedTile>();
        }
    }

    #endregion


    [Serializable]
    public class MapCache : ScriptableObject
    {
        #region Fields

        public Map map;
        public SerializableDictionary<int, MapLayer> layers;

        // Bridges
        public bool bridge_hidden;
        public List<TilePosition> cur_bridge;
        public List<List<TilePosition>> bridges;
        // Prefab Tiles
        public bool prefab_hidden;
        public GameObject cur_prefab;
        // Terrain Edges
        public bool terrain_hidden;
        public List<TilePosition> cur_terrain;
        public List<List<TilePosition>> terrain;

        public static MapCache Create(Map map)
        {
            MapCache cache = ScriptableObject.CreateInstance<MapCache>();
            cache.InitCache(map);

            return cache;
        }
        private void InitCache(Map map)
        {
            this.map = map;
            layers = new SerializableDictionary<int, MapLayer>();
            foreach (int layer_id in map.map_layers.Keys)
            {
                MapLayer map_layer = new MapLayer(layer_id);
                layers.Add(layer_id, map_layer);
            }

            // Hideable Object Tracking Initialization
            bridge_hidden = false;
            cur_bridge = null;
            bridges = new List<List<TilePosition>>();

            prefab_hidden = false;
            cur_prefab = null;

            terrain_hidden = false;
            cur_terrain = null;
            terrain = new List<List<TilePosition>>();
        }
        public override string ToString()
        {
            return map.name;
        }

        #endregion


        #region Cache Functions

        /*
        * Set tile in cache
        */
        public void SetTile(Vector3 pos, MatchedTile tile)
        {
            int layer_id; Vector2Int int_pos;
            (layer_id, int_pos) = Convert3DPosition(map, pos);

            SetTile(layer_id, int_pos, tile);
        }
        public void SetTile(int layer_id, Vector3Int pos, MatchedTile tile)
        {
            SetTile(layer_id, new Vector2Int(pos.x, pos.y), tile);
        }
        public void SetTile(int layer_id, Vector2Int pos, MatchedTile tile)
        {
            if (!layers.ContainsKey(layer_id)) return;
            layers[layer_id].tiles[pos] = tile;
        }

        /*
        * Get tile from cache
        */
        public MatchedTile GetTile(Vector3 pos)
        {
            int layer_id; Vector2Int int_pos;
            (layer_id, int_pos) = Convert3DPosition(map, pos);

            return GetTile(layer_id, int_pos);
        }
        public MatchedTile GetTile(int layer_id, Vector3Int pos)
        {
            return GetTile(layer_id, new Vector2Int(pos.x, pos.y));
        }
        public MatchedTile GetTile(int layer_id, Vector2Int pos)
        {
            if (!layers.ContainsKey(layer_id)) return new MatchedTile{};
            if (!layers[layer_id].tiles.ContainsKey(pos)) return new MatchedTile{};
            
            return layers[layer_id].tiles[pos];
        }

        /* 
        * Hideable Object Cacheing
        */
        public void CacheBridge(List<TilePosition> bridge)
        {
            bridges.Add(bridge);
        }
        public void CacheTerrain(List<TilePosition> edges)
        {
            terrain.Add(edges);
        }

        /*
        * Hideable Object Retrieval
        */
        public List<TilePosition> GetBridge(TilePosition tile_pos)
        {
            foreach (List<TilePosition> bridge in bridges)
            {
                if (bridge.Contains(tile_pos)) return bridge;
            }

            return null;
        }
        public List<TilePosition> GetTerrain(TilePosition tile_pos)
        {
            foreach (List<TilePosition> edge in terrain)
            {
                if (edge.Contains(tile_pos)) return edge;
            }

            return null;
        }

        #endregion


        #region Static Methods

        /*
        * Convert a world position in 3d to the corresponding layer and 2d position
        */
        public static (int, Vector2Int) Convert3DPosition(Map map, Vector3 pos)
        {
            int layer_id = map.GetMapLayerIDFromPosition(pos);
            Vector2Int int_pos = new Vector2Int((int)pos.x, (int)pos.y);

            return (layer_id, int_pos);
        }

        #endregion
    }
}

