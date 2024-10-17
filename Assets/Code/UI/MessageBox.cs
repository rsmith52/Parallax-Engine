using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI
{
    public class MessageBox : MonoBehaviour
    {
        #region Fields

        [Title("Message Text", bold: false)]
        [HideLabel]
        [MultiLineProperty(3)]
        public string message_text;

        #endregion
    }
}
