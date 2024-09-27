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
        // Terrain Navigation
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

        public static bool IsStairTile(ParallaxTileBase tile)
        {
            TerrainTags[] stair_tags = new TerrainTags[] { TerrainTags.StairUp, TerrainTags.StairLeft, TerrainTags.StairRight };
            return stair_tags.Contains(tile.terrain_tag);
        }

        public static bool IsWaterTile(ParallaxTileBase tile)
        {
            TerrainTags[] water_tags = new TerrainTags[] { TerrainTags.WaterStill, TerrainTags.WaterOcean, TerrainTags.DeepWater };
            return water_tags.Contains(tile.terrain_tag);
        }

        public static bool IsBridgeTile(ParallaxTileBase tile)
        {
            TerrainTags[] bridge_tags = new TerrainTags[] { TerrainTags.Bridge };
            return bridge_tags.Contains(tile.terrain_tag);
        }

        #endregion
    }
}