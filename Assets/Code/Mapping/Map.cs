using System.Collections.Generic;
using UnityEngine;

namespace Mapping
{
    #region Enums
    public enum Maps
    {

    }

    #endregion

    public class Map : MonoBehaviour
    {
        #region Fields
        public static Dictionary<Maps, Map> map_dict;

        #endregion


        #region Static Methods
        public static Map getMapByID(int id)
        {
            return map_dict[(Maps)id];
        }

        #endregion
    }
}