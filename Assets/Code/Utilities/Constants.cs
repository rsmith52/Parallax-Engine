using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public static class Constants
    {
        #region Mapping

        public static readonly float MAP_LAYER_HEIGHT = 0.5f;

        #endregion
        
        
        #region Movement

        // Movement Speeds and Turn Sensitivity
        public static readonly float[] SPEEDS = new float[]
        {
            2f,         // VerySlow
            3f,         // Slow
            3.5f,       // Moderate
            6f,         // Fast
            8f          // VeryFast
        };
        public static readonly float TAP_VS_HOLD_TIME = 0.075f;

        #endregion
    }
}