using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0264", "UNREADABLE_IDENTIFIER")]
    internal sealed class UnreadableIdentifierRule : AbstractRule
    {
        private const int MinAllowedSymbols = 2;

        private static readonly HashSet<string> ValidShortNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "i",
            "j",
            "k",
            "l",
            "m",
            "n",
            "x",
            "y",
        };

        private static readonly Regex NonWordChars = new Regex("[^a-zA-Zа-яА-Я]+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex StartWithWordChars = new Regex("^[a-zA-Zа-яА-Я]{3,}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly Action<Identifier, string> validator;

        public UnreadableIdentifierRule() : base()
        {
            validator = new Action<Identifier, string>(ValidateIdentifier);
        }

        protected override void ValidateScript(TSqlScript node) => node.AcceptChildren(new DatabaseObjectIdentifierDetector(validator, true, false, true));

        private static string SanitizeName(string name)
        {
            return NonWordChars.Replace(name, "").Trim();
        }

        private void ValidateIdentifier(Identifier node, string name)
        {
            if (node is null || string.IsNullOrEmpty(name))
            {
                return;
            }

            if (ValidShortNames.Contains(name))
            {
                return;
            }

            if (name.Length >= MinAllowedSymbols)
            {
                string sanitizedName = SanitizeName(name);

                if (sanitizedName.Length >= MinAllowedSymbols
                && sanitizedName.Length * 2 >= name.Length)
                {
                    return;
                }
            }

            if (StartWithWordChars.IsMatch(name))
            {
                return;
            }

            HandleNodeError(node, name);
        }
    }
}
