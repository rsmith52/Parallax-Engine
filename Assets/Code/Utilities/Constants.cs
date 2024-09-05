using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public static class Constants
    {
        #region Movement

        // Movement Speeds and Turn Sensitivity
        public static readonly float[] SPEEDS = new float[]
        {
            1f,         // VerySlow
            2f,         // Slow
            3f,         // Moderate
            4.5f,       // Fast
            6f          // VeryFast
        };
        public static readonly float TAP_VS_HOLD_TIME = 0.075f;

        #endregion
    }
}