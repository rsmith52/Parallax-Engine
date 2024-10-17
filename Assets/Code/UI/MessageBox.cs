using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using NLP;

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

        private bool drawing;

        #endregion


        #region Mono Behavior

        private void Start()
        {
            ui_doc = GetComponent<UIDocument>();
            root_elmt = ui_doc.rootVisualElement;
            label = root_elmt.Q<Label>(Constants.TEXT_LABEL);

            display_text = Constants.MESSAGE_BOX_RICH_TEXT;
            label.text = display_text;

            drawing = false;

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
                List<TextPiece> text_pieces = TextUtils.GetTextPieces(message_text);
                TextPiece last = text_pieces.Last();

                foreach (TextPiece piece in text_pieces)
                {
                    yield return new WaitUntil(() => !drawing);

                    switch (piece.type)
                    {
                        case TextType.Word:
                            bool is_last = (piece.Equals(last));
                            StartCoroutine(DrawWord(piece, is_last));
                            break;
                        case TextType.Code:
                            string replacement = TextUtils.TextCodeReplace(piece.code);
                            display_text += replacement;
                            break;
                        default:
                            break;
                    }
                    
                }
            }
        }

        private IEnumerator DrawWord(TextPiece word, bool last = false)
        {
            drawing = true;

            for (int i = 0; i < word.length; i++)
            {
                yield return new WaitForSeconds(1 / Settings.TEXT_SPEEDS[(int)text_speed]);
                display_text += word.text[i];
                label.text = display_text;
            }

            if (!last)
            {
                yield return new WaitForSeconds(1 / Settings.TEXT_SPEEDS[(int)text_speed]);
                display_text += " ";
            }

            drawing = false;
        }

        #endregion
    }
}
