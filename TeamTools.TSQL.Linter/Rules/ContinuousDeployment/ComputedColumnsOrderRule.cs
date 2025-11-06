using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
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

        public override void Visit(CreateTableStatement node)
        {
            if (node.SchemaObjectName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                return;
            }

            node.AcceptChildren(new TableColumnVisitor(HandleNodeError));
        }

        private class TableColumnVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> callback;
            private ColumnDefinition lastComputedColumn;
            private bool isLastComputedColumnPersisted;

            public TableColumnVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(ColumnDefinition node)
            {
                if (node.ComputedColumnExpression is null)
                {
                    if (lastComputedColumn != null)
                    {
                        // Computed columns should not be followed by regular columns
                        callback(node, lastComputedColumn.ColumnIdentifier.Value);
                    }
                }
                else
                {
                    if (lastComputedColumn != null && !isLastComputedColumnPersisted && node.IsPersisted)
                    {
                        // Persisted computed columns should come before non-persisted ones
                        callback(node, lastComputedColumn.ColumnIdentifier.Value);
                    }

                    lastComputedColumn = node;
                    isLastComputedColumnPersisted = node.IsPersisted;
                }
            }
        }
    }
}
