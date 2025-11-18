using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0778", "SPARSE_AND_NOT_SPARSE")]
    internal sealed class SparseColumnsAsSecondaryRule : AbstractRule
    {
        public SparseColumnsAsSecondaryRule() : base()
        {
        }

        public override void Visit(TableDefinition node)
        {
            var firstSparseCol = CountSparseAndOtherCols(node.ColumnDefinitions, out int sparseColCount, out int allStoredColCount);

            if (firstSparseCol is null)
            {
                return;
            }

            int nonSparseColCount = allStoredColCount - sparseColCount;

            if (sparseColCount > 0 && (nonSparseColCount >= sparseColCount))
            {
                HandleNodeError(firstSparseCol, string.Format(Strings.ViolationDetails_SparseColumnsAsSecondaryRule_CountOutOfTotal, sparseColCount.ToString(), allStoredColCount.ToString()));
            }
        }

        private static TSqlFragment CountSparseAndOtherCols(IList<ColumnDefinition> cols, out int sparseCols, out int persistentCols)
        {
            sparseCols = 0;
            persistentCols = 0;
            TSqlFragment firstSparseCol = default;

            int n = cols.Count;
            for (int i = 0; i < n; i++)
            {
                var col = cols[i];
                if (col.ComputedColumnExpression is null || col.IsPersisted)
                {
                    persistentCols++;
                }

                if (col.StorageOptions != null && col.StorageOptions.SparseOption != SparseColumnOption.None)
                {
                    sparseCols++;
                    if (firstSparseCol is null)
                    {
                        firstSparseCol = col.ColumnIdentifier;
                    }
                }
            }

            return firstSparseCol;
        }
    }
}
