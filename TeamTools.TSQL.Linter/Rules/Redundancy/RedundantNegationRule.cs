using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0286", "REDUNDANT_NEGATION")]
    internal class RedundantNegationRule : AbstractRule
    {
        private const string ErrorTemplate = "use {0} instead";

        private static readonly Lazy<IDictionary<BooleanComparisonType, string>> ReversedComparisonInstance
            = new Lazy<IDictionary<BooleanComparisonType, string>>(() => InitReversedComparisonInstance(), true);

        public RedundantNegationRule() : base()
        {
        }

        private static IDictionary<BooleanComparisonType, string> ReversedComparison => ReversedComparisonInstance.Value;

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
                HandleNodeError(node, string.Format(ErrorTemplate, isnull.IsNot ? "IS NULL" : "IS NOT NULL"));
            }
        }

        private static IDictionary<BooleanComparisonType, string> InitReversedComparisonInstance()
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

        private BooleanExpression GetNegatedExpression(BooleanExpression expr)
        {
            if (expr is BooleanParenthesisExpression p)
            {
                return GetNegatedExpression(p.Expression);
            }
            else
            {
                return expr;
            }
        }
    }
}
