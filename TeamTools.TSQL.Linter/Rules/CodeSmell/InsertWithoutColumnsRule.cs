using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0103", "INSERT_COLUMN_LIST")]
    internal sealed class InsertWithoutColumnsRule : AbstractRule
    {
        public InsertWithoutColumnsRule() : base()
        {
        }

        public override void Visit(InsertSpecification node) => ValidateColumns(node.Columns, node);

        public override void Visit(OutputIntoClause node) => ValidateColumns(node.IntoTableColumns, node);

        public override void Visit(InsertMergeAction node) => ValidateColumns(node.Columns, node);

        private void ValidateColumns(IList<ColumnReferenceExpression> cols, TSqlFragment node)
        {
            if (cols.Count == 0)
            {
                HandleNodeError(node);
            }
        }
    }
}
