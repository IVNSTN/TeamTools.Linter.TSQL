using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
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
