using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0738", "NOT_IS_NULL")]
    internal sealed class NotIsNullRule : AbstractRule
    {
        public NotIsNullRule() : base()
        {
        }

        public override void Visit(BooleanNotExpression node)
        {
            var expr = ExtractExpression(node.Expression);
            if (expr is BooleanIsNullExpression isnull && !isnull.IsNot)
            {
                HandleNodeError(node);
            }
        }

        private static BooleanExpression ExtractExpression(BooleanExpression expr)
        {
            while (expr is BooleanParenthesisExpression be)
            {
                expr = be.Expression;
            }

            return expr;
        }
    }
}
