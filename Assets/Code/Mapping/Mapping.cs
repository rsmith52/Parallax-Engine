using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mapping.Tiles;

namespace Mapping
{
    #region Enums

    public enum Maps
    {
        None = 0,
        TestMap1 = 1,
        TestMap2 = 2
    }

    public enum MapType
    {
        None = 0,
        Outdoors = 1
    }

    public enum MapRegions
    {
        None = 0,
        TidepoolTropics = 1,
        CoastOfLegends = 2,
        StarryCliffs = 3,
        SandstoneDesert = 4,
        CloudpierceCaverns = 5,
        IslesOfLoss = 6,
        SpearpointPeaks = 7,
        HexedWetlands = 8,
        DeeprootJungle = 9,
        StormySea = 10
    }

    #endregion


    #region Structs

    [Serializable]
    public struct NeighborTilemaps
    {
        public int ground_layer_id;
        
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
        [HideInInspector]
        public ParallaxTileBase down_two_tile;
        [HideInInspector]
        public ParallaxTileBase down_two_left_tile;
        [HideInInspector]
        public ParallaxTileBase down_two_right_tile;
        [HideInInspector]
        public ParallaxTileBase down_left_two_tile;
        [HideInInspector]
        public ParallaxTileBase down_right_two_tile;
    }

    [Serializable]
    public struct MatchedTile
    {
        public ParallaxTileBase tile;
        public Tilemap map;
        public Tilemap layer;
        public bool object_match;
    }

    [Serializable]
    public struct TilePosition
    {
        public Tilemap map;
        public Vector3Int pos;
        public bool is_object;

        public TilePosition (Tilemap m, Vector3Int p, bool o = false)
        {
            map = m;
            pos = p;
            is_object = o;
        }
    }

    #endregion
}
