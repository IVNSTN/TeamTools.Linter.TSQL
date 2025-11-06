using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private static void DoValidateColumnCount(int definedColCount, int selectedColCount, Action<string> callback)
        {
            if (definedColCount != selectedColCount)
            {
                callback($"{definedColCount} vs {selectedColCount}");
            }
        }

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
            if (definedColumns is null || definedColumns.Count == 0 || selectedColCount == 0)
            {
                // could not evaluate
                return;
            }

            DoValidateColumnCount(definedColumns.Count, selectedColCount, msg => HandleNodeError(definedColumns.Last(), msg));
        }
    }
}
