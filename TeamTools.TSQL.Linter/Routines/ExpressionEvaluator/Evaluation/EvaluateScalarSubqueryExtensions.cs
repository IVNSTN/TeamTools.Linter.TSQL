using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    internal static class EvaluateScalarSubqueryExtensions
    {
        public static SqlValue EvaluateScalarSubquery(this SqlExpressionEvaluator eval, ScalarSubquery subquery)
        {
            var spec = subquery.QueryExpression.GetQuerySpecification();

            if (spec is null)
            {
                return default;
            }

            if (spec.SelectElements.Count == 1 && spec.SelectElements[0] is SelectScalarExpression sse)
            {
                return eval.EvaluateExpression(sse.Expression);
            }

            return default;
        }
    }
}
