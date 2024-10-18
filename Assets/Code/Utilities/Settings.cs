using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Utilities
{
    public class Settings
    {
        #region Resource File Paths

        public static readonly string MATERIALS_PATH = "Materials";
        public static readonly string PIXEL_SNAPPING_MATERIAL_FILENAME = "Tilemap Pixel Snapping";
        public static readonly string OUTLINE_MATERIAL_FILENAME_PREFIX = "Outline - ";

        #endregion


        #region Player Abilities

        public static readonly bool ALLOW_JUMPING = true;
        public static readonly bool ALLOW_JUMP_UP_LEDGES = false;
        public static readonly bool ALLOW_JUMP_OVER_OBJECTS = false;
        public static readonly bool ALLOW_SWIMMING = true;
        public static readonly bool ALLOW_DIVING = true;

        #endregion


        #region Map Rules

        public static readonly bool ALLOW_WALK_BEHIND_TERRAIN_EDGES = false;
        
        public static readonly bool SEE_THROUGH_BRIDGES = true;
        public static readonly bool SEE_THROUGH_PREFAB_OBJECTS = true;
        public static readonly bool SEE_THROUGH_TERRAIN = false;

        #endregion


        #region UI

        public static readonly float[] TEXT_SPEEDS = new float[]
        {
            20,          // Slow
            35,          // Moderate
            50,          // Fast
            9999         // Instant
        };

        #endregion
    }
}
