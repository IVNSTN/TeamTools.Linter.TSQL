using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class BaseTemporalTableRangeValidator
    {
        protected static readonly string BestTypeForHistory = "DATETIME2";
        protected static readonly int BestPrecision = 7;

        protected static bool TryExtractDateTimePrecision(DataTypeReference dataType, out int precision)
        {
            precision = -1;

            if (!(dataType is SqlDataTypeReference systemType))
            {
                // something unknown
                return true;
            }

            if (!systemType.GetFullName().Equals(BestTypeForHistory, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (systemType.Parameters.Count != 1)
            {
                // no precision or broken syntax
                return true;
            }

            if (!int.TryParse(systemType.Parameters[0].Value, out precision))
            {
                // probably broken syntax
                precision = -1;
            }

            return true;
        }

        protected virtual bool IsDataTypeAlright(DataTypeReference dataType)
        {
            return TryExtractDateTimePrecision(dataType, out int definedPrecision)
                && (definedPrecision == -1 || definedPrecision >= BestPrecision);
        }
    }
}
