using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0249", "REDUNDANT_BRACKETS")]
    internal sealed class RedundantSquareBracketsRule : AbstractRule, IKeywordDetector
    {
        // @ removed from valid first symbols because it is only valid for variables
        private static readonly Regex ValidIdentifierRegex = new Regex(
            "^(?<first_symbol>[a-zA-Zа-яА-Я_#])(?<the_rest>[\\w$#@]+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        private HashSet<string> reservedWords;

        public RedundantSquareBracketsRule() : base()
        {
        }

        public void LoadKeywords(ICollection<string> values)
        {
            reservedWords = new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
        }

        // TODO : Support GEOMETRY, GEOGRAPHY which are parsed as UDT, not built-in type
        public override void ExplicitVisit(SqlDataTypeReference node) => ValidateIdentifiers(node.Name?.Identifiers, true);

        public override void Visit(Identifier node) => ValidateIdentifier(node);

        private void ValidateIdentifiers(IList<Identifier> identifiers, bool isEmbeddedDataType)
        {
            if (identifiers is null)
            {
                // e.g. CURSOR
                // if it was written as [CURSOR] it wouldn't get parsed as built-in cursor datatype
                return;
            }

            for (int i = 0, n = identifiers.Count; i < n; i++)
            {
                ValidateIdentifier(identifiers[i], isEmbeddedDataType);
            }
        }

        private void ValidateIdentifier(Identifier node, bool isEmbeddedDataType = false)
        {
            Debug.Assert(reservedWords != null && reservedWords.Count > 0, "reservedWords not loaded");

            if (node is null)
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

            if (string.IsNullOrEmpty(rawIdentifier))
            {
                return;
            }

            // no brackets
            if (rawIdentifier[0] != '[')
            {
                return;
            }

            if (isEmbeddedDataType)
            {
                HandleNodeError(node);
                return;
            }

            // is a registered keyword
            if (reservedWords.Contains(node.Value))
            {
                return;
            }

            // spaces in name is most used case for quoting names
            if (node.Value.Contains(' '))
            {
                return;
            }

            // not a valid identifier
            if (!ValidIdentifierRegex.IsMatch(node.Value))
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
