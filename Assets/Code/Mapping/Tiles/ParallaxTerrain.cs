using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mapping.Tiles
{
    public class ParallaxTerrain
    {
        #region Terrain Groups

        // Stair Tags
        private static TerrainTags[] up_stair_tags = new TerrainTags[] { TerrainTags.StairUp };
        private static TerrainTags[] side_stair_tags = new TerrainTags[] { TerrainTags.StairLeft, TerrainTags.StairRight };

        // Water Tags
        private static TerrainTags[] water_tags = new TerrainTags[] { TerrainTags.WaterStill };
        private static TerrainTags[] ocean_tags = new TerrainTags[] { TerrainTags.WaterOcean };
        private static TerrainTags[] deep_water_tags = new TerrainTags[] { TerrainTags.DeepWater };
        private static TerrainTags[] shore_tags = new TerrainTags[] { TerrainTags.Shore };

        // Bridge Tags
        private static TerrainTags[] bridge_tags = new TerrainTags[] { TerrainTags.Bridge };

        // Underwater Tags
        private static TerrainTags[] underwater_tags = new TerrainTags[] { TerrainTags.Underwater, TerrainTags.WaterGrass };

        // Ledge Tags
        private static TerrainTags[] ledge_tags = new TerrainTags[] { TerrainTags.Ledge };

        // Sand Tags
        private static TerrainTags[] sand_tags = new TerrainTags[] { TerrainTags.Sand };

        // Snow Tags
        private static TerrainTags[] snow_tags = new TerrainTags[] { TerrainTags.Snow };

        // Grass Tags
        private static TerrainTags[] grass_tags = new TerrainTags[] { TerrainTags.TallGrass, TerrainTags.ExtraTallGrass };
        private static TerrainTags[] water_grass_tags = new TerrainTags[] { TerrainTags.WaterGrass };

        #endregion
        

        #region Static Methods

        public static bool IsStairTile(ParallaxTileBase tile, bool is_side_only = false, bool is_up_stair_only = false)
        {
            if (tile == null) return false;
            
            if (is_side_only) return side_stair_tags.Contains(tile.terrain_tag);
            else if (is_up_stair_only) return up_stair_tags.Contains(tile.terrain_tag);
            else return (side_stair_tags.Contains(tile.terrain_tag) || up_stair_tags.Contains(tile.terrain_tag));
        }

        public static bool IsWaterTile(ParallaxTileBase tile, bool is_deep_only = false, bool is_ocean_only = false, bool see_shore = false)
        {
            if (tile == null) return false;
            
            if (is_deep_only) return deep_water_tags.Contains(tile.terrain_tag);
            else if (is_ocean_only) return ocean_tags.Contains(tile.terrain_tag);
            else if (see_shore) return (water_tags.Contains(tile.terrain_tag) || ocean_tags.Contains(tile.terrain_tag) || deep_water_tags.Contains(tile.terrain_tag) || shore_tags.Contains(tile.terrain_tag));
            else return (water_tags.Contains(tile.terrain_tag) || ocean_tags.Contains(tile.terrain_tag) || deep_water_tags.Contains(tile.terrain_tag));
        }

        public static bool IsShoreTile(ParallaxTileBase tile)
        {
            if (tile == null) return false;
            return shore_tags.Contains(tile.terrain_tag);
        }

        public static bool IsBridgeTile(ParallaxTileBase tile)
        {
            if (tile == null) return false;
            return bridge_tags.Contains(tile.terrain_tag);
        }

        public static bool IsUnderwaterTile(ParallaxTileBase tile)
        {
            if (tile == null) return false;
            return underwater_tags.Contains(tile.terrain_tag);
        }

        public static bool IsLedgeTile(ParallaxTileBase tile)
        {
            if (tile == null) return false;
            return ledge_tags.Contains(tile.terrain_tag);
        }

        public static bool IsSandTile(ParallaxTileBase tile)
        {
            if (tile == null) return false;
            return sand_tags.Contains(tile.terrain_tag);
        }

        public static bool IsSnowTile(ParallaxTileBase tile)
        {
            if (tile == null) return false;
            return snow_tags.Contains(tile.terrain_tag);
        }

        public static bool IsGrassTile(ParallaxTileBase tile, bool dry_only = false)
        {
            if (tile == null) return false;
            if (dry_only) return grass_tags.Contains(tile.terrain_tag);
            else return grass_tags.Contains(tile.terrain_tag) || water_grass_tags.Contains(tile.terrain_tag);
        }

        public static bool IsTerrainTile(ParallaxTileBase tile)
        {
            if (tile == null) return false;
            RuleTile ruletile = tile as RuleTile;
            if (ruletile != null && ruletile.is_terrain) return true;
            else return false;
        }

        #endregion
    }
}