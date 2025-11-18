using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0179", "OUTPUT_COL_DEFINITION_MISMATCH")]
    internal sealed class ParenthesisSubqueryDefinitionColCountRule : AbstractRule
    {
        public ParenthesisSubqueryDefinitionColCountRule() : base()
        {
        }

        public override void Visit(InlineDerivedTable node) => ValidateColumnCountMatch(node.Columns, CountQueryColumns(node.RowValues));

        public override void Visit(QueryDerivedTable node) => ValidateColumnCountMatch(node.Columns, CountQueryColumns(node.QueryExpression));

        public override void Visit(CommonTableExpression node) => ValidateColumnCountMatch(node.Columns, CountQueryColumns(node.QueryExpression));

        private static int CountQueryColumns(QueryExpression query)
        {
            var topQuery = query.GetQuerySpecification();
            if (topQuery is null)
            {
                return 0;
            }

            if (topQuery.ForClause != null && (topQuery.ForClause is JsonForClause || topQuery.ForClause is XmlForClause))
            {
                // for xml/json return single column
                return 1;
            }
            else
            {
                return topQuery.SelectElements.Count;
            }
        }

        private static int CountQueryColumns(IList<RowValue> rowValues)
        {
            if (rowValues is null || rowValues.Count == 0)
            {
                return 0;
            }

            return rowValues[0].ColumnValues.Count;
        }

        private void ValidateColumnCountMatch(IList<Identifier> definedColumns, int selectedColCount)
        {
            int definedColCount = definedColumns.Count;
            if (definedColumns is null || definedColCount == 0 || selectedColCount == 0)
            {
                // could not evaluate
                return;
            }

            if (definedColCount != selectedColCount)
            {
                var msg = $"{definedColCount} != {selectedColCount}";
                HandleNodeError(definedColumns[definedColCount - 1], msg);
            }
        }
    }
}
