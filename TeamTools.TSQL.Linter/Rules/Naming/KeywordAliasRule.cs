using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0714", "ALIAS_IS_KEYWORD")]
    internal sealed class KeywordAliasRule : AbstractRule, IKeywordDetector
    {
        private HashSet<string> reservedWords;

        public KeywordAliasRule() : base()
        {
        }

        public void LoadKeywords(ICollection<string> values)
        {
            reservedWords = new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
        }

        public override void Visit(SelectScalarExpression node) => ValidateAlias(node.ColumnName?.Identifier);

        public override void Visit(TableReferenceWithAlias node) => ValidateAlias(node.Alias);

        public override void Visit(CommonTableExpression node) => ValidateAlias(node.ExpressionName);

        private void ValidateAlias(Identifier name)
        {
            Debug.Assert(reservedWords != null && reservedWords.Count > 0, "reservedWords not loaded");

            if (name != null && reservedWords.Contains(name.Value))
            {
                HandleNodeError(name, name.Value);
            }
        }
    }
}
