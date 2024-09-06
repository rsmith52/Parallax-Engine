using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

namespace Mapping
{
    #region Enums
    public enum Maps
    {
        None = 0,
        TestMap1 = 1
    }

    public enum MapType
    {
        None = 0,
        Outdoors = 1
    }

    #endregion


    [Serializable]
    public class Map : MonoBehaviour
    {
        #region Global

        public static Dictionary<Maps, Map> map_dict;

        #endregion
        

        #region Fields

        [Title("Basic Information")]
        public int map_id;
        [ValidateInput("NameMatchesGameObject", "Map's name must match the game object's name.")]
        public string map_name;
        public MapType map_type;

        [Title("Layer Information")]
        public int num_layers;
        public TilemapRenderer[] map_layers;
        private TilemapRenderer[] object_layers;

        #endregion


        #region Validation Methods

        private bool NameMatchesGameObject()
        {
            return map_name.Equals(this.gameObject.name);
        }

        #endregion


        #region Static Methods

        public static Map getMapByID(int id)
        {
            return map_dict[(Maps)id];
        }

        #endregion
    }
}