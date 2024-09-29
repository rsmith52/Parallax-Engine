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

        public static bool IsStairTile(ParallaxTileBase tile, bool is_side_only = false)
        {
            if (tile == null) return false;
            TerrainTags[] stair_tags = new TerrainTags[] { TerrainTags.StairUp, TerrainTags.StairLeft, TerrainTags.StairRight };
            TerrainTags[] side_stair_tags = new TerrainTags[] { TerrainTags.StairLeft, TerrainTags.StairRight };
            return is_side_only ? side_stair_tags.Contains(tile.terrain_tag) : stair_tags.Contains(tile.terrain_tag);
        }

        public static bool IsWaterTile(ParallaxTileBase tile)
        {
            if (tile == null) return false;
            TerrainTags[] water_tags = new TerrainTags[] { TerrainTags.WaterStill, TerrainTags.WaterOcean, TerrainTags.DeepWater };
            return water_tags.Contains(tile.terrain_tag);
        }

        public static bool IsBridgeTile(ParallaxTileBase tile)
        {
            if (tile == null) return false;
            TerrainTags[] bridge_tags = new TerrainTags[] { TerrainTags.Bridge };
            return bridge_tags.Contains(tile.terrain_tag);
        }

        public static bool IsUnderwaterTile(ParallaxTileBase tile)
        {
            if (tile == null) return false;
            TerrainTags[] underwater_tags = new TerrainTags[] { TerrainTags.Underwater, TerrainTags.WaterGrass };
            return underwater_tags.Contains(tile.terrain_tag);
        }

        public static bool IsTerrainTile(ParallaxTileBase tile)
        {
            RuleTile ruletile = tile as RuleTile;
            if (ruletile != null && ruletile.is_terrain) return true;
            else return false;
        }

        #endregion
    }
}