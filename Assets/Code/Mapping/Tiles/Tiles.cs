using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mapping.Tiles
{
    #region Enums

    public enum TerrainTags
    {
        None = 0,
        // Ground Types
        Grass = 1,
        Dirt = 2,
        Rock = 3,
        Sand = 4,
        Snow = 5,
        Ice = 6,
        Puddle = 7,
        Shore = 8,
        Underwater = 9,
        // Encounters
        TallGrass = 10,
        ExtraTallGrass = 11,
        DeepSand = 12,
        DeepSnow = 28,
        Marsh = 13,
        WaterGrass = 14,
        // Terrain Navigation
        Ledge = 15,
        StairUp = 16,
        StairLeft = 17,
        StairRight = 18,
        Bridge = 19,
        // Water
        WaterStill = 20,
        WaterOcean = 21,
        DeepWater = 22,
        // Special
        Waterfall = 23,
        WaterfallCrest = 24,
        RockyLedge = 25,
        SlipperSlope = 29,
        NoPassHelper = 26,
        Ignore = 27
    }
    
    #endregion

    
    #region Structs

    [Serializable]
    public struct TileSize
    {
        public int x_width;
        public int y_height;
        public int z_layers;
    }

    #endregion
}
