using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines
{
    internal static class LookAlikeCharDetector
    {
        private static readonly char LastAsciiChar = '\u007f';

        private static readonly Dictionary<char, char[]> LookAlikes = new Dictionary<char, char[]>
        {
            { 'a', new char[] { '\u0430', '\u00e0', '\u00e1', '\u1ea1', '\u0105' } },
            { 'B', new char[] { '\u0412' } },
            { 'c', new char[] { '\u0441', '\u0188', '\u010b' } },
            { 'd', new char[] { '\u0501', '\u0257' } },
            { 'e', new char[] { '\u0435', '\u1eb9', '\u0117', '\u00e9', '\u00e8' } },
            { 'g', new char[] { '\u0121' } },
            { 'h', new char[] { '\u04bb' } },
            { 'H', new char[] { '\u041d' } },
            { 'i', new char[] { '\u0456', '\u00ed', '\u00ec', '\u00ef' } },
            { 'j', new char[] { '\u0458', '\u029d' } },
            { 'k', new char[] { '\u03ba', '\u041a' } },
            { 'l', new char[] { '\u04cf', '\u1e37' } },
            { 'm', new char[] { '\u043c', '\u039c' } },
            { 'n', new char[] { '\u0578' } },
            { 'o', new char[] { '\u043e', '\u03bf', '\u0585', '\u022f', '\u1ecd', '\u1ecf', '\u01a1', '\u00f6', '\u00f3', '\u00f2' } },
            { 'p', new char[] { '\u0440' } },
            { 'q', new char[] { '\u0566' } },
            { 's', new char[] { '\u0282' } },
            { 'T', new char[] { '\u0422' } },
            { 'u', new char[] { '\u03c5', '\u057d', '\u00fc', '\u00fa', '\u00f9' } },
            { 'v', new char[] { '\u03bd', '\u0475' } },
            { 'x', new char[] { '\u0445', '\u04b3' } },
            { 'y', new char[] { '\u0443', '\u00fd' } },
            { 'z', new char[] { '\u0290', '\u017c' } },
        };

        private static readonly Dictionary<char, char> CharMap = new Dictionary<char, char>();

        static LookAlikeCharDetector()
        {
            // generating reverse mapping
            foreach (var c in LookAlikes)
            {
                foreach (var v in c.Value)
                {
                    var upperVersion = char.ToUpper(v);

                    CharMap.Add(v, c.Key);
                    if (!CharMap.ContainsKey(upperVersion))
                    {
                        CharMap.Add(upperVersion, char.ToUpper(c.Key));
                    }
                }
            }

            // including uppercase versions
            foreach (var c in LookAlikes.Keys.ToArray())
            {
                var upperVersion = char.ToUpper(c);
                if (!LookAlikes.ContainsKey(upperVersion))
                {
                    LookAlikes.Add(upperVersion, LookAlikes[c]);
                }
            }
        }

        public static void ValidateChars(string text, int startLine, int lineOffset, Action<int, int, string> callback)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            int i = 0;
            int lookAlikePos = -1;
            int originPos = -1;
            bool hasLatin = false;
            bool hasNonLatin = false;
            bool escapeNext = false;
            bool lastWasCarriageReturn = false;

            // to prevent extra offset in first iteration
            lineOffset--;

            foreach (var c in text)
            {
                lineOffset++;

                if (c == '\r' || c == '\n')
                {
                    // line break
                    hasLatin = false;
                    hasNonLatin = false;
                    originPos = -1;
                    lookAlikePos = -1;
                    escapeNext = false;

                    if (!(lastWasCarriageReturn && c == '\n'))
                    {
                        startLine++;
                    }

                    lastWasCarriageReturn = c == '\r';
                    lineOffset = 0;
                }
                else if (c == '\\')
                {
                    // possible char escaping like \r\n\t for whatever reason
                    escapeNext = true;
                    lastWasCarriageReturn = false;
                }
                else if (escapeNext && (c == 'r' || c == 'n' || c == 't'))
                {
                    // just ignoring \r\n\t
                    hasLatin = false;
                    hasNonLatin = false;
                    originPos = -1;
                    lookAlikePos = -1;
                    escapeNext = false;
                    lastWasCarriageReturn = false;
                }
                else if (!char.IsLetterOrDigit(c))
                {
                    // word break
                    hasLatin = false;
                    hasNonLatin = false;
                    originPos = -1;
                    lookAlikePos = -1;
                    escapeNext = false;
                    lastWasCarriageReturn = false;
                }
                else if (CharMap.ContainsKey(c))
                {
                    // look-alike detected in a word
                    lookAlikePos = i;
                    escapeNext = false;
                    lastWasCarriageReturn = false;
                }
                else if (IsLatinLetter(c))
                {
                    // latin ascii letter detected in a word
                    escapeNext = false;
                    lastWasCarriageReturn = false;
                    if (LookAlikes.ContainsKey(c))
                    {
                        originPos = i;
                        // TODO : still need better violation positioning
                        // if the word is mostly non-latin - report this latin char instead of all others
                        if (!hasNonLatin)
                        {
                            hasLatin = true;
                        }
                    }
                    else
                    {
                        hasLatin = true;
                    }
                }
                else if (c > LastAsciiChar && char.IsLetter(c))
                {
                    // non-ascii letter detected in a word
                    hasNonLatin = true;
                    escapeNext = false;
                    lastWasCarriageReturn = false;
                }
                else
                {
                    escapeNext = false;
                    lastWasCarriageReturn = false;
                }

                if (hasLatin && lookAlikePos >= 0)
                {
                    callback(startLine, lineOffset, $"'{text[lookAlikePos].ToString()}' at {lookAlikePos.ToString()}");
                    lookAlikePos = -1;
                    hasNonLatin = false;
                    originPos = -1;
                }
                else if (hasNonLatin && originPos >= 0)
                {
                    callback(startLine, lineOffset, $"'{text[originPos].ToString()}' at {originPos.ToString()}");
                    originPos = -1;
                    hasLatin = false;
                    originPos = -1;
                }

                i++;
            }
        }

        private static bool IsLatinLetter(char ch)
        {
            return (ch >= 'A' && ch <= 'Z')
                || (ch >= 'a' && ch <= 'z');
        }
    }
}
