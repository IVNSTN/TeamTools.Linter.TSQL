using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0793", "INVISIBLE_CHAR")]
    internal sealed class LiteralHasInvisibleCharRule : AbstractRule
    {
        private static readonly Dictionary<char, string> InvisibleChars;

        static LiteralHasInvisibleCharRule()
        {
            // TODO : add some more
            InvisibleChars = new Dictionary<char, string>
            {
                { '\u070F', "Syriac Abbreviation Mark" },
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
                { '\u200B', "Zero-Width Space" },
                { '\u200C', "Zero Width Non-Joiner" },
                { '\u200D', "Zero Width Joiner" },
                { '\u200E', "Left-To-Right Mark" },
                { '\u200F', "Right-To-Left Mark" },
                { '\u202F', "Narrow No-Break Space" },
                { '\u205F', "Medium Mathematical Space" },
                { '\u2060', "Word Joiner" },
                { '\u2800', "Braille Pattern Blank" },
                { '\uFEFF', "Zero Width No-break Space" },
            };
        }

        public LiteralHasInvisibleCharRule() : base()
        {
        }

        public override void Visit(StringLiteral node)
        {
            if (string.IsNullOrEmpty(node.Value))
            {
                return;
            }

            const char zeroCodeChar = '\0';

            int n = node.Value.Length;
            for (int i = 0; i < n; i++)
            {
                var c = node.Value[i];
                if (c != zeroCodeChar && InvisibleChars.TryGetValue(c, out var symbolName))
                {
                    HandleNodeError(node, string.Format(Strings.ViolationDetails_LiteralHasInvisibleCharRule_SymbolAtPos, symbolName, i.ToString()));
                    return;
                }
            }
        }
    }
}
