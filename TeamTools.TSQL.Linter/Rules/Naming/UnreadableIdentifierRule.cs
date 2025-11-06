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

        private static readonly Lazy<ICollection<string>> ValidShortNamesInstance
            = new Lazy<ICollection<string>>(() => InitValidShortNamesInstance(), true);

        private readonly Regex nonWordChars = new Regex("[^a-zA-Zа-яА-Я]+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        private readonly Regex startWithWordChars = new Regex("^[a-zA-Zа-яА-Я]{3,}", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        public UnreadableIdentifierRule() : base()
        {
        }

        private ICollection<string> ValidShortNames => ValidShortNamesInstance.Value;

        public override void Visit(TSqlBatch node)
        {
            var id = new DatabaseObjectIdentifierDetector(ValidateIdentifier, true, false, true);
            node.AcceptChildren(id);
        }

        private static ICollection<string> InitValidShortNamesInstance()
        {
            return new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
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
        }

        private void ValidateIdentifier(Identifier node, string name)
        {
            if (node is null || string.IsNullOrEmpty(name))
            {
                return;
            }

            string sanitizedName = nonWordChars.Replace(name, "").Trim();

            if (sanitizedName.Length >= MinAllowedSymbols
            && sanitizedName.Length >= (name.Length / 2))
            {
                return;
            }

            if (startWithWordChars.IsMatch(name)
            || ValidShortNames.Contains(name))
            {
                return;
            }

            HandleNodeError(node, name);
        }
    }
}
