using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0728", "NOT_FOR_COMPLEX_PREDICATE")]
    internal sealed class ComplexPredicateNegationRule : AbstractRule
    {
        public ComplexPredicateNegationRule() : base()
        {
        }

        public override void Visit(BooleanNotExpression node)
        {
            if (node.Expression is BooleanBinaryExpression)
            {
                HandleNodeError(node);
            }
        }

        public override void Visit(BooleanBinaryExpression node)
        {
            if (node.FirstExpression is BooleanNotExpression
            && !(node.SecondExpression is BooleanParenthesisExpression)
            && !(node.SecondExpression is BooleanNotExpression))
            {
                HandleNodeError(node);
            }
        }
    }
}
