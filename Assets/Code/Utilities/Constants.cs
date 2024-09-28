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
        public static readonly int EVENT_SORTING_LAYER_OFFSET = 5;
        public static readonly string EXTRA_PRIORITY_TILE_TAG = "Extra Priority Tile";
        public static readonly string PRIORITY_TILE_TAG = "Priority Tile";
        public static readonly string DEPRIORITY_TILE_TAG = "DePriority Tile";
        public static readonly string UP_LAYER_PRIORITY_TILE = "Up Layer Priority Tile";
        public static readonly int PRIORITY_TILE_OFFSET = (int)(0.1f * SORTING_LAYERS_PER_MAP_LAYER);
        public static readonly float TRANS_TILE_ALPHA = 0.75f;

        #endregion
        
        
        #region Movement

        // Movement Speeds and Turn Sensitivity
        public static readonly float[] SPEEDS = new float[]
        {
            2f,         // VerySlow (Sneak Speed)
            3f,         // Slow
            3.5f,       // Moderate (Jump in Place Speed)
            6f,         // Fast     (Jump Speed)
            8f          // VeryFast (Run Speed)
        };
        public static readonly float TAP_VS_HOLD_TIME = 0.075f;
        public static readonly float JUMP_HEIGHT = 0.5f;

        #endregion


        #region Sprites

        public static readonly string SHADOW_TAG = "Shadow";

        #endregion
    }
}