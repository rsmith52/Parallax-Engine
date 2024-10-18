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

        private bool drawing_line;
        private bool drawing_word;
        private bool generating_lines;
        private bool calculating_width;

        private List<List<TextPiece>> lines;

        #endregion


        #region Mono Behavior

        private void Start()
        {
            ui_doc = GetComponent<UIDocument>();
            root_elmt = ui_doc.rootVisualElement;
            label = root_elmt.Q<Label>(Constants.TEXT_LABEL);

            display_text = Constants.MESSAGE_BOX_RICH_TEXT;
            label.text = display_text;

            drawing_line = false;
            drawing_word = false;
            calculating_width = false;

            if (auto_start) StartCoroutine(DrawText());
        }

        #endregion


        #region Text Drawing

        [Button("Draw Text")]
        [HideIf("@drawing_word")]
        public void StartDrawingText()
        {
            StartCoroutine(DrawText());
        }

        public IEnumerator DrawText(string text = "")
        {
            if (text != "") message_text = text;
            List<TextPiece> text_pieces = TextUtils.GetTextPieces(message_text);

            StartCoroutine(GenerateTextLines(text_pieces));
            yield return new WaitUntil(() => !generating_lines);

            display_text = Constants.MESSAGE_BOX_RICH_TEXT;
            label.text = display_text;

            foreach (List<TextPiece> line in lines)
            {
                yield return new WaitUntil(() => !drawing_line);
                StartCoroutine(DrawLine(line));
            }
        }

        private IEnumerator DrawLine(List<TextPiece> line)
        {
            drawing_line = true;

            TextPiece last = line.Last();

            foreach (TextPiece piece in line)
            {
                yield return new WaitUntil(() => !drawing_word);
                bool is_last = (piece.Equals(last));
                string replacement;

                switch (piece.type)
                {
                    case TextType.Word:
                    case TextType.RichWord:
                        StartCoroutine(DrawWord(piece, is_last));
                        break;
                    case TextType.Variable:
                        replacement = TextUtils.VariableReplace(piece.text);
                        StartCoroutine(DrawWord(new TextPiece(replacement), is_last));
                        break;
                    case TextType.Code:
                        replacement = TextUtils.TextCodeReplace(piece.code);
                        display_text += replacement;
                        break;
                    case TextType.Rich:
                        drawing_word = true;
                        display_text += piece.text;
                        drawing_word = false;
                        break;
                    case TextType.RichVariable:
                        replacement = TextUtils.VariableReplace(piece.text);
                        StartCoroutine(DrawWord(new TextPiece(replacement, piece.rich_mods, true)));
                        break;
                    default:
                        break;
                }
            }

            drawing_line = false;
        }

        private IEnumerator DrawWord(TextPiece word, bool last = false)
        {
            drawing_word = true;

            string mod_start = "";
            string mod_end = "";
            if (word.type == TextType.RichWord || word.type == TextType.RichVariable)
            {
                mod_start = TextUtils.ConstructTextModStart(word.rich_mods);
                mod_end = TextUtils.ConstructTextModEnd(word.rich_mods);
            }

            for (int i = 0; i < word.length; i++)
            {
                yield return new WaitForSeconds(1 / Settings.TEXT_SPEEDS[(int)text_speed]);

                if (word.type == TextType.RichWord || word.type == TextType.RichVariable)
                {
                    if (i == 0) display_text += mod_start; // add start of rich text
                    else display_text = display_text.Substring(0, display_text.LastIndexOf(mod_end)); // remove end of rich text from unfinished word
                }
                
                display_text += word.text[i];

                if (word.type == TextType.RichWord || word.type == TextType.RichVariable)
                    display_text += mod_end;

                label.text = display_text;
            }

            if (!last)
            {
                yield return new WaitForSeconds(1 / Settings.TEXT_SPEEDS[(int)text_speed]);
                display_text += " ";
            }

            drawing_word = false;
        }

        private IEnumerator GenerateTextLines(List<TextPiece> text_pieces)
        {
            generating_lines = true;

            lines = new List<List<TextPiece>>();
            List<TextPiece> cur_line = new List<TextPiece>();
            float max_line_width = label.parent.resolvedStyle.width;
            float piece_width;
            float cur_line_width = 0f;
            bool new_line;

            foreach (TextPiece piece in text_pieces)
            {
                new_line = false;

                switch (piece.type)
                {
                    case TextType.Word:
                    case TextType.RichWord:
                        label.text = piece.text + " ";

                        label.RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
                        calculating_width = true;
                        yield return new WaitUntil(() => !calculating_width);
                        piece_width = label.resolvedStyle.width;

                        break;
                    case TextType.Variable:
                    case TextType.RichVariable:
                        label.text = TextUtils.VariableReplace(piece.text);

                        label.RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
                        calculating_width = true;
                        yield return new WaitUntil(() => !calculating_width);
                        piece_width = label.resolvedStyle.width;

                        break;
                    case TextType.Code:
                        if (piece.code == TextCodes.NewLine)
                        {
                            new_line = true;
                            piece_width = 0f;
                        }
                        else
                        {
                            label.text = TextUtils.TextCodeReplace(piece.code);

                            label.RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
                            calculating_width = true;
                            yield return new WaitUntil(() => !calculating_width);
                            piece_width = label.resolvedStyle.width;
                        }
                        break;
                    case TextType.Rich:
                        piece_width = 0f;
                        break;
                    default:
                        piece_width = 0f;
                        break;
                }

                Debug.Log(cur_line_width);
                new_line = new_line || (cur_line_width + piece_width > max_line_width);
                if (new_line)
                {
                    lines.Add(new List<TextPiece>(cur_line));
                    cur_line = new List<TextPiece> { piece };
                    cur_line_width = piece_width;
                }
                else
                {
                    cur_line.Add(piece);
                    cur_line_width += piece_width;
                }
            }
            lines.Add(new List<TextPiece>(cur_line));

            label.text = "";
            generating_lines = false;
        }

        private void GeometryChangedCallback(GeometryChangedEvent evt)
        {
            label.UnregisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
            calculating_width = false;
        }

        #endregion
    }
}
