using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0773", "MAX_COL_SPARSE")]
    internal sealed class MaxColumnSparseRule : AbstractRule
    {
        public MaxColumnSparseRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node)
        {
            if (node.StorageOptions is null || node.StorageOptions.SparseOption == SparseColumnOption.None)
            {
                return;
            }

            if (!(node.DataType is ParameterizedDataTypeReference dt)
            || dt.Parameters.Count != 1)
            {
                return;
            }

            if (dt.Parameters[0] is MaxLiteral max)
            {
                HandleNodeError(max, node.ColumnIdentifier.Value);
            }
        }
    }
}
