using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0824", "SMALL_SIZE_UNICODE")]
    internal sealed class SmallSizeColumnUnicodeRule : AbstractRule
    {
        private static readonly int DefaultVarcharLength = 30;
        private static readonly int MinSizeToMakeSense = 12; // just some magic

        public SmallSizeColumnUnicodeRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node)
        {
            if (!(node.DataType is SqlDataTypeReference colType))
            {
                return;
            }

            if (!(colType.SqlDataTypeOption == SqlDataTypeOption.NChar
            || colType.SqlDataTypeOption == SqlDataTypeOption.NVarChar))
            {
                // NTEXT is big, not small
                return;
            }

            int size;

            if (colType.Parameters.Count == 0)
            {
                size = DefaultVarcharLength;
            }
            else if (colType.Parameters[0] is MaxLiteral)
            {
                return;
            }
            else if (!int.TryParse(colType.Parameters[0].Value, out size))
            {
                // something broken
                return;
            }

            if (size >= MinSizeToMakeSense)
            {
                return;
            }

            HandleNodeError(node.DataType, $"{node.ColumnIdentifier.Value}({size})");
        }
    }
}
