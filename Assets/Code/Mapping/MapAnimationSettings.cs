using UnityEngine;
using Sirenix.OdinInspector;
using Utilities;

namespace Mapping
{
    public class MapAnimationSettings : SerializedMonoBehaviour
    {
        #region Animation Settings

        public float appear_delay = 0.1f;
        public float kill_delay = 0.25f;
        public Vector3 anim_angle = Constants.DEFAULT_PERSPECTIVE_ANGLE;

        #endregion
    }
}

