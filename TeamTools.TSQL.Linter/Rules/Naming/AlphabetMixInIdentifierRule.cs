using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0259", "ALPHABET_MIX_IDENTIFIER")]
    internal sealed class AlphabetMixInIdentifierRule : AbstractRule
    {
        private readonly Regex digitsOnly = MakeRegex(@"^[$@$\s0-9_+)(-]+$");
        private readonly Regex latinSymbols = MakeRegex(@"[a-zA-Z$]+");
        private readonly Regex nonLatinSymbols = MakeRegex(@"[^a-zA-Z0-9_@#&$\/\\\s.,?:-]+");
        private readonly Regex cyrillicSymbols = MakeRegex(@"[а-яА-Я]+");
        private readonly Regex nonCyrillicSymbols = MakeRegex(@"[^а-яА-Я0-9_@#&$\/\\\s.,?:-]+");

        public AlphabetMixInIdentifierRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var ident = new DatabaseObjectIdentifierDetector(ValidateIdentifier, true);
            node.AcceptChildren(ident);
        }

        private static Regex MakeRegex(string pattern)
            => new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private void ValidateIdentifier(Identifier node, string name)
        {
            string ident = node?.Value;

            if (string.IsNullOrEmpty(ident))
            {
                return;
            }

            if (digitsOnly.IsMatch(ident))
            {
                // digit-only or unreadable identifier which is handled by separate rule
                return;
            }

            bool isLatin = latinSymbols.IsMatch(ident);
            if (!nonLatinSymbols.IsMatch(ident) && isLatin)
            {
                return;
            }

            bool isCyrillic = cyrillicSymbols.IsMatch(ident);
            if (!nonCyrillicSymbols.IsMatch(ident) && isCyrillic)
            {
                return;
            }

            if (!isLatin && !isCyrillic)
            {
                // unknown language
                return;
            }

            HandleNodeError(node, ident);
        }
    }
}
