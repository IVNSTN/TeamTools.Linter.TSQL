using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics.CodeAnalysis;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("SI0727", "NOT_FOR_SIMPLE_PREDICATE")]
    internal sealed class SimplePredicateNegationRule : AbstractRule
    {
        public SimplePredicateNegationRule() : base()
        {
        }

        public override void Visit(BooleanNotExpression node)
        {
            var expr = node.Expression;
            while (expr is BooleanParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            if (expr is BooleanComparisonExpression cmp)
            {
                var revertedComparison = ComparisonToString(RevertComparison(cmp.ComparisonType));

                HandleNodeError(cmp, string.Format(Strings.ViolationDetails_SimplePredicateNegationRule_UseReversedComparison, revertedComparison));
            }
        }

        [ExcludeFromCodeCoverage]
        private static BooleanComparisonType RevertComparison(BooleanComparisonType cmp)
        {
            switch (cmp)
            {
                case BooleanComparisonType.Equals:
                    return BooleanComparisonType.NotEqualToBrackets;

                case BooleanComparisonType.NotEqualToBrackets:
                case BooleanComparisonType.NotEqualToExclamation:
                    return BooleanComparisonType.Equals;

                case BooleanComparisonType.GreaterThan:
                    return BooleanComparisonType.LessThanOrEqualTo;

                case BooleanComparisonType.GreaterThanOrEqualTo:
                    return BooleanComparisonType.LessThan;

                case BooleanComparisonType.LessThanOrEqualTo:
                    return BooleanComparisonType.GreaterThan;

                case BooleanComparisonType.LessThan:
                    return BooleanComparisonType.GreaterThanOrEqualTo;

                case BooleanComparisonType.NotGreaterThan:
                    return BooleanComparisonType.GreaterThan;

                case BooleanComparisonType.NotLessThan:
                    return BooleanComparisonType.LessThan;

                // TODO : or fail?
                default:
                    return cmp;
            }
        }

        // TODO : extract to something more reusable
        [ExcludeFromCodeCoverage]
        private static string ComparisonToString(BooleanComparisonType cmp)
        {
            switch (cmp)
            {
                case BooleanComparisonType.Equals:
                    return "=";

                case BooleanComparisonType.NotEqualToBrackets:
                    return "<>";

                case BooleanComparisonType.NotEqualToExclamation:
                    return "!=";

                case BooleanComparisonType.GreaterThan:
                    return ">";

                case BooleanComparisonType.GreaterThanOrEqualTo:
                    return ">=";

                case BooleanComparisonType.LessThanOrEqualTo:
                    return "<=";

                case BooleanComparisonType.LessThan:
                    return "<";

                case BooleanComparisonType.NotGreaterThan:
                    return "!>";

                case BooleanComparisonType.NotLessThan:
                    return "!<";

                // TODO : or fail?
                default:
                    return default;
            }
        }
    }
}
