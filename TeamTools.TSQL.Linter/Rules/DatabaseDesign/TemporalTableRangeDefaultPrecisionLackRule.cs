using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0865", "HISTORY_PERIOD_DEFAULT_PRECISION")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    internal sealed class TemporalTableRangeDefaultPrecisionLackRule : BaseTemporalTableRangeValidator, ISqlServerMetadataConsumer
    {
        private IDictionary<string, string> dateFunctions;

        public TemporalTableRangeDefaultPrecisionLackRule()
        {
        }

        // Storing output types of system datetime functions
        public void LoadMetadata(SqlServerMetadata data)
        {
            dateFunctions = data.Functions
                .Where(f => f.Value.DataType != null && f.Value.DataType.Contains("DATE", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(f => f.Key, f => f.Value.DataType, StringComparer.OrdinalIgnoreCase);
        }

        protected override bool DoesColumnHaveCorrectDefinition(ColumnDefinition col) => IsDefaultAlright(col);

        protected override bool IsDefaultAlright(ColumnDefinition col)
        {
            var def = ExtractDefaultConstraint(col);

            if (def is null)
            {
                return true;
            }

            var defaultValueType = GetOutputType(ExpandExpression(def.Expression), out int defaultPrecision);

            if (string.IsNullOrEmpty(defaultValueType))
            {
                // something unsupported/unrecognizable
                return true;
            }

            if (!string.Equals(defaultValueType, BestTypeForHistory, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (TryExtractDateTimePrecision(col.DataType, out int columnPrecision)
            && columnPrecision == -1)
            {
                // default precision for DATETIME2
                columnPrecision = 7;
            }

            // Rule was unable to determine either of precisions or DEFAULT precision is fine
            return defaultPrecision == -1 || columnPrecision == -1 || defaultPrecision >= columnPrecision;
        }

        private string GetOutputType(ScalarExpression expr, out int precision)
        {
            precision = -1; // no precision set

            if (expr is null)
            {
                return default;
            }

            if (expr is CastCall cast)
            {
                TryExtractDateTimePrecision(cast.DataType, out precision);
                return cast.DataType.GetFullName();
            }

            if (expr is ConvertCall convert)
            {
                TryExtractDateTimePrecision(convert.DataType, out precision);
                return convert.DataType.GetFullName();
            }

            if (expr is FunctionCall func)
            {
                if (func.FunctionName.Value.Equals("DATEADD", StringComparison.OrdinalIgnoreCase)
                && func.Parameters.Count == 3)
                {
                    // depends on what we are "date adding" to
                    return GetOutputType(ExpandExpression(func.Parameters[2]), out precision);
                }

                if (dateFunctions.TryGetValue(func.FunctionName.Value, out var outputType))
                {
                    return outputType;
                }
            }

            return default;
        }
    }
}
