using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    #region Enums

    public enum TextSpeeds
    {
        Slow,
        Moderate,
        Fast,
        Instant
    }

    public enum TextType
    {
        Word,
        Code
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

        public TextPiece(string text)
        {
            this.text = text;
            length = text.Length;
            type = TextType.Word;
            code = TextCodes.None;
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