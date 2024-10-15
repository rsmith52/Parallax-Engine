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

