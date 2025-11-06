using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0281", "UNIQUE_DERIVED_DEF_COL_NAME")]
    public sealed class UniqueDerivedColumnAliasRule : AbstractRule
    {
        public UniqueDerivedColumnAliasRule() : base()
        {
        }

        public override void Visit(QueryDerivedTable node) => ValidateColumns(node.Columns);

        public override void Visit(CommonTableExpression node) => ValidateColumns(node.Columns);

        public override void Visit(InlineDerivedTable node)
        {
            if (node.RowValues.Count == 0)
            {
                return;
            }

            ValidateColumns(node.Columns);
        }

        private void ValidateColumns(IList<Identifier> columns)
        {
            if (columns is null || columns.Count == 0)
            {
                return;
            }

            ValidateColDefinition(columns);
        }

        private void ValidateColDefinition(IList<Identifier> cols)
        {
            ICollection<string> names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var col in cols)
            {
                if (!names.TryAddUnique(col.Value))
                {
                    // name already in use
                    HandleNodeError(col);
                }
            }
        }
    }
}
