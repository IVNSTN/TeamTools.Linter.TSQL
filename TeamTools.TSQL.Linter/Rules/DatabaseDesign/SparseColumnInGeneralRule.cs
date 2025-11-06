using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0777", "SPARSE_COL_USED")]
    internal sealed class SparseColumnInGeneralRule : AbstractRule
    {
        public SparseColumnInGeneralRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node)
        {
            if (node.StorageOptions != null && node.StorageOptions.SparseOption != SparseColumnOption.None)
            {
                HandleNodeError(node.StorageOptions, node.ColumnIdentifier.Value);
            }
        }
    }
}
