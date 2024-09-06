using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Mapping
{
    public class MapLayer : MonoBehaviour
    {
        #region Fields

        [ReadOnly]
        public Map map;
        [ReadOnly]
        public int layer;

        #endregion


        #region Mono Behavior

        private void Start()
        {
            map = GetComponentInParent<Map>();
        }

        #endregion
    }
}