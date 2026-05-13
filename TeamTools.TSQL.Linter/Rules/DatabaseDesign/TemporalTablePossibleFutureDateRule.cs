using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0864", "HISTORY_FUTURE_DATE")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    internal sealed partial class TemporalTablePossibleFutureDateRule : BaseTemporalTableRangeValidator
    {
        public TemporalTablePossibleFutureDateRule() : base()
        {
        }

        // TODO : Should it check the case when for DATETIME(7) default has DATEADD with positive argument?
        protected override bool IsDefaultAlright(DefaultConstraintDefinition def)
        {
            const int dateAddArgCount = 3;
            const string dateAddFunction = "DATEADD";

            if (def is null)
            {
                // no default
                return true;
            }

            var defValue = ExpandExpression(def.Expression);

            if (defValue is Literal)
            {
                return true;
            }

            // DATEADD with negative offset
            return
                defValue is FunctionCall fn
                && string.Equals(fn.FunctionName.Value, dateAddFunction, StringComparison.OrdinalIgnoreCase)
                && fn.Parameters.Count == dateAddArgCount
                && ExpandExpression(fn.Parameters[1]) is UnaryExpression ue
                && ue.UnaryExpressionType == UnaryExpressionType.Negative
                && ExpandExpression(ue.Expression) is IntegerLiteral literal
                && int.TryParse(literal.Value, out int dateAddValue)
                && dateAddValue != 0;
        }

        protected override bool DoesColumnDefineTemporalRange(ColumnDefinition col, string startCol, string endCol)
        {
            // Only the Start column cannot accept future date
            return col.ColumnIdentifier.Value.Equals(startCol, StringComparison.OrdinalIgnoreCase);
        }
    }
}
