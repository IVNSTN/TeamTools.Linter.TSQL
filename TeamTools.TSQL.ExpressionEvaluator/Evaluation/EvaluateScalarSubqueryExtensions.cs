using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.ExpressionEvaluator.Core;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
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
