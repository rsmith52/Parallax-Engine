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
        #region Text Out

        public static List<TextPiece> GetTextPieces(string text)
        {
            string[] pieces = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            List<TextPiece> text_pieces = new List<TextPiece>();

            bool contains_rich_text = ContainsRichText(text);
            bool contains_variable = ContainsVariable(text);

            List<string> active_rich_text_mods = new List<string>();
            List<string> end_rich_text_mods = new List<string>();

            foreach (string piece in pieces)
            {
                // Gather all pure rich text and put at the front
                if (contains_rich_text && IsPureRichText(piece))
                {
                    text_pieces.Insert(0, new TextPiece(piece, TextType.Rich));
                    continue;
                }

                // Extract start/end of rich text modifiers
                else if (contains_rich_text && ContainsRichText(piece))
                {
                    active_rich_text_mods.AddRange(ExtractRichTextStartMods(piece)); // add all new mods
                    end_rich_text_mods = ExtractRichTextEndMods(piece); // track mods to remove after this piece
                }

                TextCodes code = GetTextCode(piece);

                // Recognize variables to be inserted
                if (contains_variable && ContainsVariable(piece))
                {
                    if (active_rich_text_mods.Count() > 0)
                        text_pieces.Add(new TextPiece(CleanRichText(piece), active_rich_text_mods, true));
                    else text_pieces.Add(new TextPiece(piece, TextType.Variable));
                }

                // Recognize special text codes
                else if (code != TextCodes.None)
                    text_pieces.Add(new TextPiece(piece, code));

                // Plaintext word case
                else
                {
                    if (active_rich_text_mods.Count() > 0)
                        text_pieces.Add(new TextPiece(CleanRichText(piece), active_rich_text_mods));
                    else text_pieces.Add(new TextPiece(piece));
                }

                // Update rich text mod tracking with any ended mods
                foreach (string text_mod in end_rich_text_mods)
                    active_rich_text_mods = RemoveLastMatchingMod(active_rich_text_mods, text_mod);
                end_rich_text_mods = new List<string>();
            }

            return text_pieces;
        }

        private static string GetWord(string text, int char_index)
        {
            if (text[char_index] == ' ') return "";

            string left = text.Substring(0, char_index).Split(' ').Last();
            string right = text.Substring(char_index).Split(' ').First();

            return left + right;
        }

        private static TextCodes GetTextCode(string text, int char_index)
        {
            string word = GetWord(text, char_index);
            return GetTextCode(word);
        }

        private static TextCodes GetTextCode(string text)
        {
            TextCodes code = TextCodes.None;

            if (Constants.TEXT_CODES.Contains(text))
            {
                int code_idx = Array.IndexOf(Constants.TEXT_CODES, text);
                code = (TextCodes)code_idx;
            }

            return code;
        }

        private static bool ContainsRichText(string text)
        {
            string pattern = ".*<[^<>]*>.*";
            Regex rx = new Regex(pattern);
            return rx.IsMatch(text);
        }

        private static bool IsPureRichText(string text)
        {
            string pattern = "^<[^<>/]*>$";
            Regex rx = new Regex(pattern);
            return rx.IsMatch(text);
        }

        private static List<string> ExtractRichTextStartMods(string text)
        {
            List<string> text_mods = new List<string>();

            string pattern = "<[^<>/]*>";
            Regex rx = new Regex(pattern);
            MatchCollection matches = rx.Matches(text);

            foreach (Match match in matches)
            {
                string mod = ExtractRichTextValue(match.Value);
                text_mods.Add(mod);
            }

            return text_mods;
        }

        private static List<string> ExtractRichTextEndMods(string text)
        {
            List<string> text_mods = new List<string>();

            string pattern = "</[^<>/]*>";
            Regex rx = new Regex(pattern);
            MatchCollection matches = rx.Matches(text);

            foreach (Match match in matches)
            {
                string mod = ExtractRichTextValue(match.Value);
                text_mods.Add(mod);
            }

            return text_mods;
        }

        private static string CleanRichText(string text)
        {
            string pattern = "<[^<>]*>";
            Regex rx = new Regex(pattern);

            return rx.Replace(text, "");
        }

        private static string ExtractRichTextValue(string text)
        {
            string pattern = "[<>/]";
            Regex rx = new Regex(pattern);

            return rx.Replace(text, "");
        }

        private static string ExtractRichTextLabel(string text)
        {
            string pattern = "=\".*\"";
            Regex rx = new Regex(pattern);

            return rx.Replace(text, "");
        }

        private static List<string> RemoveLastMatchingMod(List<string> mod_list, string mod)
        {
            int match_idx = -1;
            for (int i = mod_list.Count() - 1; i >= 0; i--)
            {
                string mod_label = ExtractRichTextLabel(mod_list[i]);
                if (mod_label.Equals(mod))
                {
                    match_idx = i;
                    break;
                }
            }

            if (match_idx < 0) return mod_list;
            else
            {
                mod_list.RemoveAt(match_idx);
                return mod_list;
            }
        }

        public static string ConstructTextModStart(List<string> text_mods)
        {
            string output = "";
            foreach (string mod in text_mods)
                output += "<" + mod + ">";

            return output;
        }

        public static string ConstructTextModEnd(List<string> text_mods)
        {
            string output = "";
            foreach (string mod in text_mods)
                output += "</" + ExtractRichTextLabel(mod) + ">";

            return output;
        }

        private static bool ContainsVariable(string text)
        {
            string pattern = ".*\\{.*\\}.*";
            Regex rx = new Regex(pattern);
            return rx.IsMatch(text);
        }

        private static bool IsVariable(string text)
        {
            string pattern = "^\\{.*\\}$";
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

        public static string VariableReplace(string text)
        {
            return "<VAR>";
        }

        #endregion
    }
}