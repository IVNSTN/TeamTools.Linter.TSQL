using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0269", "COLUMN_TYPE_ALIGNED")]
    internal sealed class ColumnTypeAlignedRule : AbstractRule
    {
        private static readonly int MaxViolationsPerTable = 1;

        public ColumnTypeAlignedRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node) => ValidateTableDefinition(node.Definition, node.Definition?.ColumnDefinitions);

        public override void Visit(DeclareTableVariableBody node) => ValidateTableDefinition(node.Definition, node.Definition.ColumnDefinitions);

        public override void Visit(CreateTypeTableStatement node) => ValidateTableDefinition(node.Definition, node.Definition.ColumnDefinitions);

        private void ValidateTableDefinition(TableDefinition statement, IList<ColumnDefinition> cols)
        {
            if (statement is null || cols is null || cols.Count == 0)
            {
                // e.g. FILETABLE
                return;
            }

            if (statement.ScriptTokenStream[statement.FirstTokenIndex].Line == statement.ScriptTokenStream[statement.LastTokenIndex].Line)
            {
                // one-line statements are ignored
                return;
            }

            ValidateColumnTypePosition(cols);
        }

        private void ValidateColumnTypePosition(IList<ColumnDefinition> cols)
        {
            int n = cols.Count;
            int violations = 0;
            int typeStartCol = -1;

            for (int i = 0; i < n && violations < MaxViolationsPerTable; i++)
            {
                var col = cols[i];
                if (typeStartCol == -1)
                {
                    if (col.DataType != null)
                    {
                        // Take first met column with datatype defined as the expected position
                        // for all other columns
                        typeStartCol = col.DataType.StartColumn;
                    }
                }
                else if (col.DataType != null && col.DataType.StartColumn != typeStartCol)
                {
                    HandleNodeError(col.DataType, col.ColumnIdentifier.Value);
                    violations++;
                }
            }
        }
    }
}
