using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CD0215", "COMPUTED_COLS_ORDER")]
    internal sealed class ComputedColumnsOrderRule : AbstractRule
    {
        public ComputedColumnsOrderRule() : base()
        {
        }

        public override void ExplicitVisit(CreateTableStatement node)
        {
            if (node.SchemaObjectName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                // the rule is related to persistent tables only
                return;
            }

            if (node.AsFileTable)
            {
                // filetable has no columns
                return;
            }

            ValidateDefinition(node.Definition);
        }

        private void ValidateDefinition(TableDefinition node)
        {
            ColumnDefinition lastComputedColumn = null;
            bool isLastComputedColumnPersisted = false;

            for (int i = 0, n = node.ColumnDefinitions.Count; i < n; i++)
            {
                var col = node.ColumnDefinitions[i];

                if (col.ComputedColumnExpression is null)
                {
                    if (lastComputedColumn != null)
                    {
                        // Computed columns should not be followed by regular columns
                        HandleNodeError(lastComputedColumn, lastComputedColumn.ColumnIdentifier.Value);
                    }
                }
                else
                {
                    if (lastComputedColumn != null && !isLastComputedColumnPersisted && col.IsPersisted)
                    {
                        // Persisted computed columns should come before non-persisted ones
                        HandleNodeError(node, lastComputedColumn.ColumnIdentifier.Value);
                    }

                    lastComputedColumn = col;
                    isLastComputedColumnPersisted = col.IsPersisted;
                }
            }
        }
    }
}
