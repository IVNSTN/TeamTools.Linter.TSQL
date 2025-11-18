using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0259", "ALPHABET_MIX_IDENTIFIER")]
    internal sealed class AlphabetMixInIdentifierRule : AbstractRule
    {
        private static readonly Regex DigitsOnly = MakeRegex(@"^[$@$\s0-9_+)(-]+$");
        private static readonly Regex LatinSymbols = MakeRegex(@"[a-zA-Z$]+");
        private static readonly Regex NonLatinSymbols = MakeRegex(@"[^a-zA-Z0-9_@#&$\/\\\s.,?:-]+");
        private static readonly Regex CyrillicSymbols = MakeRegex(@"[а-яА-Я]+");
        private static readonly Regex NonCyrillicSymbols = MakeRegex(@"[^а-яА-Я0-9_@#&$\/\\\s.,?:-]+");

        private readonly Action<Identifier, string> validator;

        public AlphabetMixInIdentifierRule() : base()
        {
            validator = new Action<Identifier, string>(ValidateIdentifier);
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var ident = new DatabaseObjectIdentifierDetector(validator, true);
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

            if (DigitsOnly.IsMatch(ident))
            {
                // digit-only or unreadable identifier which is handled by separate rule
                return;
            }

            bool isLatin = LatinSymbols.IsMatch(ident);
            if (!NonLatinSymbols.IsMatch(ident) && isLatin)
            {
                return;
            }

            bool isCyrillic = CyrillicSymbols.IsMatch(ident);
            if (!NonCyrillicSymbols.IsMatch(ident) && isCyrillic)
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
