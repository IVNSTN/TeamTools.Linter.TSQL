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
            private readonly Action<TSqlFragment, string> callback;
            private readonly SchemaObjectName table;
            private readonly ConstraintNameBuilder nameBuilder;

            public TableColumnVisitor(SchemaObjectName table, ConstraintNameBuilder nameBuilder, Action<TSqlFragment, string> callback) : base()
            {
                this.callback = callback;
                this.table = table;
                this.nameBuilder = nameBuilder;
            }

            public override void Visit(ColumnDefinition node)
            {
                // TODO : no need to recreate it for every column
                // visitor pattern does not seem necessary here as well
                var constraintVisitor = new ConstraintVisitor(table, node.ColumnIdentifier, nameBuilder, callback);
                node.AcceptChildren(constraintVisitor);
            }
        }
    }
}
