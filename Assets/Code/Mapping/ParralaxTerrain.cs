using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mapping
{

    #region Enums
    public enum TerrainTags
    {
        None = 0,
        // Ground Types
        Grass,
        Dirt,
        Rock,
        Sand,
        Snow,
        Ice,
        Puddle,
        Shore,
        // Encounters
        TallGrass,
        ExtraTallGrass,
        DeepSand,
        Marsh,
        WaterGrass,
        // Up / Down Navigation
        Ledge,
        StairUp,
        StairLeft,
        StairRight,
        Bridge,
        // Water
        WaterStill,
        WaterOcean,
        DeepWater,
        // Special
        Waterfall,
        WaterfallCrest,
        RockyLedge,
        Ignore
    }

    #endregion

    public class ParallaxTerrain
    {
        
    }
}