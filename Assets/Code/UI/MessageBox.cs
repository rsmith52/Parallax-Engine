using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

namespace UI
{
    public class MessageBox : MonoBehaviour
    {
        #region Fields

        private UIDocument ui_doc;
        private VisualElement root_elmt;
        private Label label;

        public TextSpeeds text_speed = TextSpeeds.Moderate;
        public bool auto_start = false;

        [Title("Message Text", bold: false)]
        [HideLabel]
        [MultiLineProperty(3)]
        public string message_text;
        private string display_text;

        #endregion


        #region Mono Behavior

        private void Start()
        {
            ui_doc = GetComponent<UIDocument>();
            root_elmt = ui_doc.rootVisualElement;
            label = root_elmt.Q<Label>(Constants.TEXT_LABEL);
            display_text = Constants.MESSAGE_BOX_RICH_TEXT;
            label.text = display_text;

            if (auto_start) StartCoroutine(DrawText());
        }

        #endregion


        #region Text Drawing

        [Button("Draw Text")]
        public void StartDrawingText()
        {
            StartCoroutine(DrawText());
        }

        public IEnumerator DrawText(string text = "")
        {
            display_text = Constants.MESSAGE_BOX_RICH_TEXT;
            label.text = display_text;
            if (text != "") message_text = text;

            if (text_speed == TextSpeeds.Instant)
            {
                display_text += message_text;
                label.text = display_text;
            }
            else
            {
                for (int i = 0; i < message_text.Length; i++)
                {
                    yield return new WaitForSeconds(1 / Settings.TEXT_SPEEDS[(int)text_speed]);

                    TextCodes code = TextUtils.GetTextCode(message_text, i);
                    string word = TextUtils.GetWord(message_text, i);

                    display_text += message_text[i];
                    label.text = display_text;
                }
            }
        }

        #endregion
    }
}
