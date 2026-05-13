using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0863", "HISTORY_SAME_DATE_RANGE_PRECISION")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    internal sealed class TemporalTableSimilarDateRangePrecision : BaseTemporalTableRangeValidator
    {
        public TemporalTableSimilarDateRangePrecision() : base()
        {
        }

        protected override void ValidateHistoryColumns(IList<ColumnDefinition> cols, string startCol, string endCol)
        {
            DataTypeReference startType = null;
            DataTypeReference endType = null;

            for (int i = cols.Count - 1; i >= 0; i--)
            {
                var col = cols[i];

                if (col.ColumnIdentifier.Value.Equals(startCol, StringComparison.OrdinalIgnoreCase))
                {
                    startType = col.DataType;
                }
                else if (col.ColumnIdentifier.Value.Equals(endCol, StringComparison.OrdinalIgnoreCase))
                {
                    endType = col.DataType;
                }

                if (startType != null && endType != null)
                {
                    break;
                }
            }

            if (startType?.Name is null || endType?.Name is null)
            {
                // probably broken syntax or something incompatible
                return;
            }

            if (!DataTypesMatch(startType, endType))
            {
                HandleNodeError(endType);
            }
        }

        private static bool DataTypesMatch(DataTypeReference startType, DataTypeReference endType)
        {
            if (!string.Equals(startType.GetFullName(), endType.GetFullName(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!(startType is SqlDataTypeReference start))
            {
                // it must be system type
                return false;
            }

            if (!(endType is SqlDataTypeReference end))
            {
                // it must be system type
                return false;
            }

            // 7 is the default precision for DATETIME2
            int startPrecision = 7;
            int endPrecision = 7;

            if (start.Parameters.Count == 1 && int.TryParse(start.Parameters[0].Value, out int p1))
            {
                startPrecision = p1;
            }

            if (end.Parameters.Count == 1 && int.TryParse(end.Parameters[0].Value, out int p2))
            {
                endPrecision = p2;
            }

            return startPrecision == endPrecision;
        }
    }
}
