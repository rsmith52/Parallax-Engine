using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NLP
{
    #region Enums

    public enum TextType
    {
        Word,
        Variable,
        Code,
        Rich,
        RichWord,
        RichVariable
    }

    public enum TextCodes
    {
        None,
        NewLine
    }

    #endregion


    #region Structs

    public struct TextPiece
    {
        public string text;
        public int length;
        public TextType type;
        public TextCodes code;
        public List<string> rich_mods;

        public TextPiece(string text, TextType type = TextType.Word)
        {
            this.text = text;
            length = text.Length;
            this.type = type;
            code = TextCodes.None;
            rich_mods = new List<string>();
        }

        public TextPiece(string text, TextCodes code)
        {
            this.text = text;
            length = text.Length;
            type = TextType.Code;
            this.code = code;
            rich_mods = new List<string>();
        }

        public TextPiece(string text, List<string> rich_mods, bool is_variable = false)
        {
            this.text = text;
            length = text.Length;
            type = is_variable ? TextType.RichVariable : TextType.RichWord;
            code = TextCodes.None;
            this.rich_mods = new List<string>();
            foreach (string mod in rich_mods)
                this.rich_mods.Add(mod);
        }
    }

    #endregion
}
