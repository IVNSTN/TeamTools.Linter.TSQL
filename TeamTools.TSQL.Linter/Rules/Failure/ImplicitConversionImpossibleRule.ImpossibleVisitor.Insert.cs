using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// INSERT statement validation for ImplicitConversionImpossibleRule.
    /// </summary>
    internal partial class ImpossibleVisitor
    {
        public override void Visit(InsertSpecification node)
        {
            if (node.InsertSource is ExecuteInsertSource)
            {
                // INSERT-EXEC has no from part with columns selected
                return;
            }

            if ((node.Columns?.Count ?? 0) == 0)
            {
                // TODO : remember order of columns in table definition and use it
                return;
            }

            string targetName = node.Target.GetFullName();

            if (string.IsNullOrEmpty(targetName))
            {
                return;
            }

            if (node.InsertSource is ValuesInsertSource val)
            {
                ValidateInsertValues(targetName, node.Columns, val);
            }
            else if (node.InsertSource is SelectInsertSource sel)
            {
                ValidateInsertSelect(targetName, node.Columns, sel);
            }
        }

        private void ValidateInsertValues(string targetName, IList<ColumnReferenceExpression> cols, ValuesInsertSource val)
        {
            if (val.RowValues.Count == 0)
            {
                return;
            }

            int rc = val.RowValues.Count;
            // required equality is handled by a separate rule
            int n = val.RowValues[0].ColumnValues.Count > cols.Count ? cols.Count : val.RowValues[0].ColumnValues.Count;

            for (int colIndex = 0; colIndex < n; colIndex++)
            {
                string columnType = typeEvaluator.GetColumnType(targetName, cols[colIndex].MultiPartIdentifier.GetLastIdentifier().Value);

                if (string.IsNullOrEmpty(columnType))
                {
                    continue;
                }

                for (int rowIndex = 0; rowIndex < rc; rowIndex++)
                {
                    ValidateCanConvertAtoB(val.RowValues[rowIndex].ColumnValues[colIndex], columnType);
                }
            }
        }

        private void ValidateInsertSelect(string targetName, IList<ColumnReferenceExpression> cols, SelectInsertSource sel)
        {
            var spec = sel.Select.GetQuerySpecification();
            if (spec is null)
            {
                return;
            }

            // required equality is handled by a separate rule
            int n = spec.SelectElements.Count > cols.Count ? cols.Count : spec.SelectElements.Count;
            for (int colIndex = 0; colIndex < n; colIndex++)
            {
                string columnType = typeEvaluator.GetColumnType(targetName, cols[colIndex].MultiPartIdentifier.GetLastIdentifier().Value);

                if (string.IsNullOrEmpty(columnType))
                {
                    continue;
                }

                if (!(spec.SelectElements[colIndex] is SelectScalarExpression selectScalar))
                {
                    continue;
                }

                ValidateCanConvertAtoB(selectScalar.Expression, columnType);
            }
        }
    }
}
