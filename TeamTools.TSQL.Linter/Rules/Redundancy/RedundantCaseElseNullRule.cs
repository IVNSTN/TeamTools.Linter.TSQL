using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0293", "REDUNDANT_CASE_ELSE_NULL")]
    internal sealed class RedundantCaseElseNullRule : AbstractRule
    {
        public RedundantCaseElseNullRule() : base()
        {
        }

        public override void Visit(CaseExpression node)
        {
            if (node.ElseExpression != null && IsNullValuedExpression(node.ElseExpression))
            {
                HandleNodeError(node.ElseExpression);
            }
        }

        private static bool IsNullValuedExpression(ScalarExpression expr)
        {
            while (expr is ParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            if (expr is BinaryExpression b)
            {
                return IsNullValuedExpression(b.FirstExpression)
                    && IsNullValuedExpression(b.SecondExpression);
            }

            if (expr is ScalarSubquery q
                && q.QueryExpression is QuerySpecification qs
                && qs.SelectElements.Count == 1
                && qs.SelectElements[0] is SelectScalarExpression sc)
            {
                return IsNullValuedExpression(sc.Expression);
            }

            return expr is NullLiteral;
        }
    }
}
