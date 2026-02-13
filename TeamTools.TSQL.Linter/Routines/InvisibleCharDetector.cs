using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines
{
    internal static class InvisibleCharDetector
    {
        private static readonly char ZeroCodeChar = '\u007f';
        private static readonly Dictionary<char, string> InvisibleChars = new Dictionary<char, string>
        {
            { '\u000B', "Line tabulation" },
            { '\u000C', "Form feed" },
            { '\u070F', "Syriac Abbreviation Mark" },
            { '\u180B', "Mongolian Free Variation Selector One" },
            { '\u180C', "Mongolian Free Variation Selector Two" },
            { '\u180D', "Mongolian Free Variation Selector Three" },
            { '\u180E', "Mongolian Vowel Separator" },
            { '\u2000', "En Quad" },
            { '\u2001', "Em Quad" },
            { '\u2002', "En Space" },
            { '\u2003', "Em Space" },
            { '\u2004', "Three-Per-Em Space" },
            { '\u2005', "Four-Per-Em Space" },
            { '\u2006', "Six-Per-Em Space" },
            { '\u2007', "Figure Space" },
            { '\u2008', "Punctuation Space" },
            { '\u2009', "Thin Space" },
            { '\u200A', "Hair Space" },
            { '\u200B', "Zero Width Space" },
            { '\u200C', "Zero Width Non-Joiner" },
            { '\u200D', "Zero Width Joiner" },
            { '\u200E', "Left To Right Mark" },
            { '\u200F', "Right To Left Mark" },
            { '\u202A', "Left To Right Embedding" },
            { '\u202B', "Right To Left Embedding" },
            { '\u202C', "Pop Directional Formatting" },
            { '\u202D', "Left To Right Override" },
            { '\u202E', "Right To Left Override" },
            { '\u202F', "Narrow No-Break Space" },
            { '\u205F', "Medium Mathematical Space" },
            { '\u2060', "Word Joiner" },
            { '\u2062', "Invisible Times" },
            { '\u2063', "Invisible Separator" },
            { '\u2064', "Invisible Plus" },
            { '\u2065', "Invisible Operators" },
            { '\u2800', "Braille Pattern Blank" },
            { '\uFEFF', "Zero Width No-break Space" },
        };

        public static int LocateInvisibleChar(string value, out string charDescription)
        {
            charDescription = null;

            if (string.IsNullOrEmpty(value))
            {
                return -1;
            }

            int n = value.Length;
            for (int i = 0; i < n; i++)
            {
                var c = value[i];
                if (c != ZeroCodeChar && InvisibleChars.TryGetValue(c, out var symbolName))
                {
                    charDescription = symbolName;
                    return i;
                }
            }

            return -1;
        }
    }
}
