using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0714", "ALIAS_IS_KEYWORD")]
    internal sealed class KeywordAliasRule : AbstractRule, IKeywordDetector
    {
        private readonly ICollection<string> reservedWords = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        public KeywordAliasRule() : base()
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

        public override void Visit(SelectScalarExpression node) => ValidateAlias(node.ColumnName?.Identifier);

        public override void Visit(TableReferenceWithAlias node) => ValidateAlias(node.Alias);

        public override void Visit(CommonTableExpression node) => ValidateAlias(node.ExpressionName);

        private void ValidateAlias(Identifier name)
        {
            if (name != null && reservedWords.Contains(name.Value))
            {
                HandleNodeError(name, name.Value);
            }
        }
    }
}
