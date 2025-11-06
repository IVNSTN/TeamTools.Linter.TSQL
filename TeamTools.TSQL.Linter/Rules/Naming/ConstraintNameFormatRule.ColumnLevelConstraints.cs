using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Column-level constraints.
    /// </summary>
    internal sealed partial class ConstraintNameFormatRule : AbstractRule
    {
        private class TableColumnVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> handleNodeError;
            private readonly SchemaObjectName table;
            private readonly ConstraintNameBuilder nameBuilder;

            public TableColumnVisitor(SchemaObjectName table, ConstraintNameBuilder nameBuilder, Action<TSqlFragment, string> errorCallback) : base()
            {
                this.handleNodeError = errorCallback;
                this.table = table;
                this.nameBuilder = nameBuilder;
            }

            public override void Visit(ColumnDefinition node)
            {
                var constraintVisitor = new ConstraintVisitor(table, node.ColumnIdentifier, nameBuilder, handleNodeError);
                node.AcceptChildren(constraintVisitor);
            }
        }
    }
}
