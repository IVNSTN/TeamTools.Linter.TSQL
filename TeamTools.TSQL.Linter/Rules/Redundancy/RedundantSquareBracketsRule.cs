using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0249", "REDUNDANT_BRACKETS")]
    internal sealed class RedundantSquareBracketsRule : AbstractRule, IKeywordDetector
    {
        private static readonly char[] Brackets = new char[] { '[', ']' };

        // @ removed from valid first symbols because it is only valid for variables
        private readonly Regex validIdentifierRegex = new Regex(
            "^(?<first_symbol>[a-zA-Zа-яА-Я_#])(?<the_rest>[\\w$#@]+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        private readonly ICollection<string> reservedWords = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        public RedundantSquareBracketsRule() : base()
        {
        }

        public void LoadKeywords(ICollection<string> values)
        {
            reservedWords.Clear();

            foreach (string keyword in values.Distinct())
            {
                reservedWords.Add(keyword);
            }
        }

        public override void Visit(TSqlScript node)
        {
            var ident = new DatabaseObjectIdentifierDetector(ValidateIdentifier, false);
            node.AcceptChildren(ident);
        }

        private void ValidateIdentifier(Identifier node, string cleanedName)
        {
            if (null == node)
            {
                return;
            }

            if (string.IsNullOrEmpty(cleanedName))
            {
                return;
            }

            // some parser bug
            if (node.FirstTokenIndex < 0)
            {
                return;
            }

            // node.Value contains unquoted text
            // and supposing the whole identifier was parsed as a single token which is most likely
            string rawIdentifier = node.ScriptTokenStream[node.FirstTokenIndex].Text;

            // no brackets
            if (rawIdentifier.Trim(Brackets).Equals(rawIdentifier))
            {
                return;
            }

            // not a valid identifier
            // using cleanedName which is different for variables because no other names can start with @
            if (!validIdentifierRegex.IsMatch(cleanedName))
            {
                return;
            }

            // is a registered keyword
            if (reservedWords.Contains(node.Value))
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
