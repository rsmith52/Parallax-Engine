using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        Underwater,
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
        NoPassHelper,
        Ignore
    }

    #endregion

    public class ParallaxTerrain
    {
        #region Static Methods

        public static bool IsStairTile(TerrainTags terrain_tag)
        {
            TerrainTags[] stair_tags = new TerrainTags[] { TerrainTags.StairUp, TerrainTags.StairLeft, TerrainTags.StairRight };
            return stair_tags.Contains(terrain_tag);
        }

        public static bool IsWaterTile(TerrainTags terrain_tag)
        {
            TerrainTags[] water_tags = new TerrainTags[] { TerrainTags.WaterStill, TerrainTags.WaterOcean, TerrainTags.DeepWater };
            return water_tags.Contains(terrain_tag);
        }

        #endregion
    }
}