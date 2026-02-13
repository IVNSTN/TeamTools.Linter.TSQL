using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

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
                var revertedComparison = BooleanComparisonConverter.ComparisonToString(
                    BooleanComparisonConverter.RevertComparison(cmp.ComparisonType));

                HandleNodeError(cmp, string.Format(Strings.ViolationDetails_SimplePredicateNegationRule_UseReversedComparison, revertedComparison));
            }
        }
    }
}
