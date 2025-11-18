using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0286", "REDUNDANT_NEGATION")]
    internal class RedundantNegationRule : AbstractRule
    {
        private static readonly string ErrorTemplate = Strings.ViolationDetails_RedundantNegationRule_InsteadUse;

        private static readonly Lazy<Dictionary<BooleanComparisonType, string>> ReversedComparisonInstance
            = new Lazy<Dictionary<BooleanComparisonType, string>>(() => InitReversedComparisonInstance(), true);

        public RedundantNegationRule() : base()
        {
        }

        private static Dictionary<BooleanComparisonType, string> ReversedComparison => ReversedComparisonInstance.Value;

        public override void Visit(BooleanNotExpression node)
        {
            var expr = GetNegatedExpression(node.Expression);

            if (expr is BooleanComparisonExpression compare)
            {
                HandleNodeError(node, string.Format(
                    ErrorTemplate,
                    ReversedComparison.GetValueOrDefault(compare.ComparisonType, compare.ComparisonType.ToString())));
            }
            else if (expr is BooleanIsNullExpression isnull)
            {
                HandleNodeError(node, string.Format(ErrorTemplate, RevertedIsNullToString(isnull.IsNot)));
            }
        }

        private static string RevertedIsNullToString(bool isNotNull) => isNotNull ? "IS NULL" : "IS NOT NULL";

        private static Dictionary<BooleanComparisonType, string> InitReversedComparisonInstance()
        {
            return new Dictionary<BooleanComparisonType, string>
            {
                { BooleanComparisonType.Equals, "<>" },
                { BooleanComparisonType.GreaterThan, "<=" },
                { BooleanComparisonType.GreaterThanOrEqualTo, "<" },
                { BooleanComparisonType.LessThan, ">=" },
                { BooleanComparisonType.LessThanOrEqualTo, ">" },
                { BooleanComparisonType.NotLessThan, ">=" },
                { BooleanComparisonType.NotGreaterThan, "<=" },
                { BooleanComparisonType.NotEqualToExclamation, "=" },
                { BooleanComparisonType.NotEqualToBrackets, "=" },
            };
        }

        private static BooleanExpression GetNegatedExpression(BooleanExpression expr)
        {
            while (expr is BooleanParenthesisExpression p)
            {
                expr = p.Expression;
            }

            return expr;
        }
    }
}
