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

        #endregion


        #region Map Effects

        public static readonly bool SEE_THROUGH_BRIDGES = true;
        public static readonly bool SEE_THROUGH_TERRAIN = true;
        public static readonly bool SEE_THROUGH_PREFAB_OBJECTS = true;

        #endregion
    }
}
