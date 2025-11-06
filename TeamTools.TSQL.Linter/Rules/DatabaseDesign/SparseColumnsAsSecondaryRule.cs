using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

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
            var sparseCols = node.ColumnDefinitions
                .Where(col => col.StorageOptions != null && col.StorageOptions.SparseOption != SparseColumnOption.None);

            int sparseColCount = sparseCols.Count();
            var sparseCol = sparseCols.FirstOrDefault();

            int allStoredColCount = node.ColumnDefinitions
                .Count(col => col.ComputedColumnExpression is null || col.IsPersisted);
            int nonSparseColCount = allStoredColCount - sparseColCount;

            if (sparseColCount > 0 && (nonSparseColCount >= sparseColCount))
            {
                HandleNodeError(sparseCol, $"{sparseColCount} out of {allStoredColCount}");
            }
        }
    }
}
