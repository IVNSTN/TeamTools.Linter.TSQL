using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0263", "KEYWORD_IDENTIFIER")]
    internal sealed class KeywordIdentifierRule : AbstractRule, IKeywordDetector
    {
        private readonly ICollection<string> reservedWords = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        public KeywordIdentifierRule() : base()
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

        public override void Visit(TSqlBatch node)
           => node.AcceptChildren(new DatabaseObjectIdentifierDetector(ValidateIdentifier, true, true, false));

        private void ValidateIdentifier(Identifier node, string name)
        {
            if (node is null || string.IsNullOrEmpty(name))
            {
                return;
            }

            if (reservedWords.Contains(name))
            {
                HandleNodeError(node, name);
            }
        }
    }
}
