using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines
{
    internal static class BooleanExpressionPartsExtractor
    {
        public static BooleanExpression ExtractExpression(BooleanExpression expr)
        {
            while (expr is BooleanParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            return expr;
        }

        public static ScalarExpression ExtractExpression(ScalarExpression expr)
        {
            while (expr is ParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            if (expr is ScalarSubquery q)
            {
                var spec = q.QueryExpression.GetQuerySpecification();
                if (spec != null
                && spec.FromClause is null
                && spec.WhereClause is null
                && spec.SelectElements.Count == 1
                && spec.SelectElements[0] is SelectScalarExpression sel)
                {
                    // if subquery is selecting single column with no WHERE and FROM
                    // then we can grab selected scalar expression itself
                    return ExtractExpression(sel.Expression);
                }
            }

            return expr;
        }
    }
}
