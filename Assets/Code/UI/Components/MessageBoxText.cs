using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components
{
    [UxmlElement]
    public partial class MessageBoxText : Label
    {
        #region Fields

        public static BindingId keyProperty = nameof(message_text);

        [UxmlAttribute]
        public string message_text;

        #endregion

        public MessageBoxText()
        {

        }

    }
}