using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public static class Constants
    {
        #region Mapping

        public static readonly float MAP_LAYER_HEIGHT = 0.5f;
        public static readonly int SORTING_LAYERS_PER_MAP_LAYER = 10;
        
        public static readonly string GROUND_LAYER_TAG = "Ground Layer";
        public static readonly string TERRAIN_EDGE_TILE_TAG = "Terrain Edge Tile";
        public static readonly string TERRAIN_CORNER_EDGE_TILE_TAG = "Terrain Corner Edge Tile";
        public static readonly string TERRAIN_EDGE_CORNER_TILE_TAG = "Terrain Edge Corner Tile"; // yes these names suck
        public static readonly string WATER_TILE_TAG = "Water Tile";
        public static readonly string TOP_SIDE_STAIR_TILE = "Top Side Stair Tile";
        
        public static readonly string PRIORITY_TILE_TAG = "Priority Tile";
        public static readonly string EXTRA_PRIORITY_TILE_TAG = "Extra Priority Tile";
        public static readonly string COVER_PRIORITY_TILE_TAG = "Cover Priority Tile";
        public static readonly string UP_LAYER_PRIORITY_TILE_TAG = "Up Layer Priority Tile";
        public static readonly string DEPRIORITY_TILE_TAG = "DePriority Tile";
        public static readonly string EXTRA_DEPRIORITY_TILE_TAG = "Extra DePriority Tile";
        public static readonly string DOWN_LAYER_PRIORITY_TILE_TAG = "Down Layer Priority Tile";
        
        public static readonly int TERRAIN_EDGE_TILE_OFFSET = 2;
        public static readonly int WATER_TILE_OFFSET = -1;
        public static readonly int PRIORITY_TILE_OFFSET = 2;
        public static readonly int OBJECT_LAYER_START_OFFSET = 3;
        public static readonly int EVENT_SORTING_LAYER_OFFSET = SORTING_LAYERS_PER_MAP_LAYER + 1;
        public static readonly int GROUND_ANIM_SORTING_LAYER_OFFSET = EVENT_SORTING_LAYER_OFFSET - 3;
        public static readonly int ANIM_SORTING_LAYER_OFFSET = EVENT_SORTING_LAYER_OFFSET;

        public static readonly float TRANS_TILE_ALPHA = 0.75f;
        public static readonly float LIGHT_TRANS_TILE_ALPHA = 0.5f;
        public static readonly float HIDDEN_LAYER_TILE_ALPHA = 0.5f;
        public static readonly float FADE_DURATION = 0.25f;

        public static readonly int MAP_SORTING_LAYER_ID = -611510623;
        public static readonly string MAP_SORTING_LAYER_NAME = "Map";
        public static readonly int ENV_SORTING_LAYER_ID = -473575885;
        public static readonly string ENV_SORTING_LAYER_NAME = "Environment";
        public static readonly int OBJ_SORTING_LAYER_ID = -242371227;
        public static readonly string OBJ_SORTING_LAYER_NAME = "Objects & Characters";

        public static readonly Vector3 WORLD_ANGLE = new Vector3(0, 0, 0);
        public static readonly Vector3 DEFAULT_PERSPECTIVE_ANGLE = new Vector3 (-15, 0, 0); // Changing this will break visuals for reflections and many 3d tiles
        public static readonly Vector3 UNDERWATER_PERSPECTIVE_ANGLE = new Vector3(0, 0, 0); // Have played with setting x up to 5, but think this is better
        public static readonly float ROTATION_SMOOTHING_CUTOFF = 1.5f; // Should increase roughly by 1 for each 5 added to underwater x angle
        public static readonly Vector3 DEFAULT_SPRITE_POS = new Vector3(0.5f, 1, 0);
        public static readonly Vector3 DEFAULT_BUSH_MASK_POS = new Vector3(0, -0.75f, 0);
        public static readonly Vector3 UNDERWATER_SPRITE_OFFSET = new Vector3(0, 0.5f, 0);
        public static readonly Vector3 UNDERWATER_BUSH_MASK_OFFSET = new Vector3(0, -0.25f, 0);

        #endregion


        #region Movement

        // Movement Speeds and Turn Sensitivity
        public static readonly float[] SPEEDS = new float[]
        {
            1.75f,      // VerySlow (Sneak Speed)
            3f,         // Slow
            3.5f,       // Moderate (Jump in Place Speed)
            6f,         // Fast     (Jump Speed)
            8f          // VeryFast (Run Speed)
        };
        public static readonly float TAP_VS_HOLD_TIME = 0.075f;
        public static readonly float JUMP_HEIGHT = 0.5f;

        #endregion


        #region Sprites

        public static readonly string SPRITE_TAG = "Sprite";
        public static readonly string SHADOW_TAG = "Shadow";
        public static readonly string REFLECTION_TAG = "Reflection";

        #endregion


        #region Animation

        public static readonly string ANIM_DIRECTION = "Direction";
        public static readonly string ANIM_WALK = "Walk";
        public static readonly string ANIM_RUN = "Run";
        public static readonly string ANIM_SNEAK = "Sneak";
        public static readonly string ANIM_JUMP = "Jump";

        #endregion


        #region UI

        public static readonly string TEXT_LABEL = "Text";

        public static readonly string MESSAGE_BOX_RICH_TEXT = "<line-height=160%>";

        public static readonly string[] TEXT_CODES = new string[]
        {
            "",         // None
            "\\n"        // NewLine
        };

        #endregion
    }
}