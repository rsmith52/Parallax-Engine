using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;

namespace Utilities
{
    public class TextUtils
    {
        #region static methods

        public static List<TextPiece> GetStringPieces(string text)
        {
            string[] pieces = text.Split(' ');
            List<TextPiece> text_pieces = new List<TextPiece>();

            foreach (string piece in pieces)
            {
                TextCodes code = GetTextCode(piece);
                if (code == TextCodes.None)
                    text_pieces.Add(new TextPiece(piece));
                else
                    text_pieces.Add(new TextPiece(piece, code));
            }

            return text_pieces;
        }

        public static string GetWord(string text, int char_index)
        {
            if (text[char_index] == ' ') return "";

            string left = text.Substring(0, char_index).Split(' ').Last();
            string right = text.Substring(char_index).Split(' ').First();

            return left + right;
        }

        public static TextCodes GetTextCode(string text)
        {
            TextCodes code = TextCodes.None;

            if (Constants.TEXT_CODES.Contains(text))
            {
                int code_idx = Array.IndexOf(Constants.TEXT_CODES, text);
                code = (TextCodes)code_idx;
            }

            return code;
        }

        public static TextCodes GetTextCode(string text, int char_index)
        {
            string word = GetWord(text, char_index);
            return GetTextCode(word);
        }

        #endregion
    }
}