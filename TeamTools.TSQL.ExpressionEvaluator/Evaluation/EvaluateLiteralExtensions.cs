using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.ExpressionEvaluator.Core;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    internal static class EvaluateLiteralExtensions
    {
        // TODO : preceding target type definition detection is preferred
        public static SqlValue EvaluateLiteral(
            this SqlExpressionEvaluator eval,
            Literal src)
        {
            if (src is NullLiteral)
            {
                return eval.LiteralValueFactory.MakeNull(src);
            }

            return eval.LiteralValueFactory.Make(src);
        }
    }
}
