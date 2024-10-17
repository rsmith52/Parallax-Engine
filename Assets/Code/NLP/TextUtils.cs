using System;
using System.Linq;
using System.Collections.Generic;
using Utilities;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NLP
{
    public class TextUtils
    {
        #region Raw String Parsing

        public static List<TextPiece> GetTextPieces(string text)
        {
            string[] pieces = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            List<TextPiece> text_pieces = new List<TextPiece>();

            bool contains_rich_text = ContainsRichText(text);
            bool contains_variable = ContainsVariable(text);

            foreach (string piece in pieces)
            {
                // Gather all rich text and put at the front
                if (contains_rich_text && IsRichText(piece))
                {
                    text_pieces.Insert(0, new TextPiece(piece, TextType.Rich));
                    continue;
                }

                // Recognize variables to be inserted
                else if (contains_variable && IsVariable(piece))
                {
                    text_pieces.Add(new TextPiece(piece, TextType.Variable));
                    continue;
                }

                // Recognize special text codes
                TextCodes code = GetTextCode(piece);
                if (code != TextCodes.None)
                    text_pieces.Add(new TextPiece(piece, code));

                // Simple text case
                else text_pieces.Add(new TextPiece(piece));
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

        public static TextCodes GetTextCode(string text, int char_index)
        {
            string word = GetWord(text, char_index);
            return GetTextCode(word);
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

        public static bool ContainsRichText(string text)
        {
            string pattern = ".*<.*>.*";
            Regex rx = new Regex(pattern);
            return rx.IsMatch(text);
        }

        public static bool IsRichText(string text)
        {
            string pattern = "<.*>";
            Regex rx = new Regex(pattern);
            return rx.IsMatch(text);
        }

        public static bool ContainsVariable(string text)
        {
            string pattern = ".*\\{.*\\}.*";
            Regex rx = new Regex(pattern);
            return rx.IsMatch(text);
        }

        public static bool IsVariable(string text)
        {
            string pattern = "\\{.*\\}";
            Regex rx = new Regex(pattern);
            return rx.IsMatch(text);
        }

        #endregion


        #region Text Code Replacement

        public static string TextCodeReplace(TextCodes code)
        {
            switch (code)
            {
                case TextCodes.NewLine:
                    return System.Environment.NewLine;
                case TextCodes.None:
                default:
                    return "";
            }
        }

        #endregion
    }
}