using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public static class Controls
    {
        #region Movement

        public static readonly KeyCode MOVE_UP = KeyCode.UpArrow;
        public static readonly KeyCode MOVE_LEFT = KeyCode.LeftArrow;
        public static readonly KeyCode MOVE_RIGHT = KeyCode.RightArrow;
        public static readonly KeyCode MOVE_DOWN = KeyCode.DownArrow;

        public static readonly KeyCode JUMP = KeyCode.Space;

        public static readonly KeyCode RUN_BUTTON = KeyCode.X;
        public static readonly KeyCode SNEAK_BUTTON = KeyCode.C;

        #endregion

    
        #region Interactions

        public static readonly KeyCode OPEN_MENU = KeyCode.Return;
        public static readonly KeyCode ACTION_BUTTON = KeyCode.Z;

        #endregion
        
        
        #region Debug

        public static readonly KeyCode SHIFT_LAYER_UP = KeyCode.W;
        public static readonly KeyCode SHIFT_LAYER_DOWN = KeyCode.S;
        public static readonly KeyCode MOVE_THROUGH_WALLS = KeyCode.LeftShift;
        
        #endregion
    }
}