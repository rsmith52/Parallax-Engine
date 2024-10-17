using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NLP
{
    #region Enums

    public enum TextType
    {
        Word,
        Code,
        Rich,
        Variable
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

        public TextPiece(string text, TextType type = TextType.Word)
        {
            this.text = text;
            length = text.Length;
            code = TextCodes.None;
            this.type = type;
        }

        public TextPiece(string text, TextCodes code)
        {
            this.text = text;
            length = text.Length;
            type = TextType.Code;
            this.code = code;
        }
    }

    #endregion
}
