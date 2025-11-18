using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0772", "SMALL_COL_SPARSE")]
    internal sealed class SmallSizeColumnSparseRule : AbstractRule
    {
        private static readonly int MaxSizeForVarSizeType = 12; // just some magic

        private static readonly HashSet<string> VarSizeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CHAR",
            "NCHAR",
            "VARCHAR",
            "NVARCHAR",
            "BINARY",
            "VARBINARY",
        };

        private static readonly HashSet<string> SmallTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BIT",
            "TINYINT",
            "SMALLINT",
            "DATE",
            "TIME",
            "SMALLDATETIME",
        };

        public SmallSizeColumnSparseRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node)
        {
            if (node.StorageOptions is null || node.StorageOptions.SparseOption == SparseColumnOption.None)
            {
                return;
            }

            string columnType = node.DataType?.Name?.BaseIdentifier.Value;

            if (string.IsNullOrEmpty(columnType))
            {
                // computed col
                return;
            }

            if (SmallTypes.Contains(columnType))
            {
                HandleNodeError(node.DataType, node.ColumnIdentifier.Value);
            }
            else if (VarSizeTypes.Contains(columnType)
                && node.DataType is ParameterizedDataTypeReference pdt
                && pdt.Parameters.Count == 1
                && pdt.Parameters[0] is IntegerLiteral i
                && int.TryParse(i.Value, out int value)
                && value <= MaxSizeForVarSizeType)
            {
                HandleNodeError(node.DataType, node.ColumnIdentifier.Value);
            }
        }
    }
}
