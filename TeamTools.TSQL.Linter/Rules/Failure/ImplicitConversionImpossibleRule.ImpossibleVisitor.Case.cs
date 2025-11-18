using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// CASE expression validation for ImplicitConversionImpossibleRule.
    /// </summary>
    internal partial class ImpossibleVisitor
    {
        // CASE result type is the type with highest precision out of all THEN/ELSE expressions
        public override void Visit(SearchedCaseExpression node)
        {
            var thenExpressions = node.ExtractOutputExpressions().ToArray();
            var resultingType = typeEvaluator.GetExpressionType(thenExpressions);
            if (string.IsNullOrEmpty(resultingType))
            {
                return;
            }

            // all THEN results must be compatible with CASE output type
            foreach (var then in thenExpressions)
            {
                // this will evaluate ELSE clause too
                ValidateCanConvertAtoB(then, resultingType);
            }
        }

        // CASE result type is the type with highest precision out of all THEN/ELSE expressions
        // also in CASE @val WHEN ... all WHEN expressions must be compatible with input expression
        public override void Visit(SimpleCaseExpression node)
        {
            var inputType = typeEvaluator.GetExpressionType(node.InputExpression);
            // all WHEN values are compatible when CASE input expression
            if (!string.IsNullOrEmpty(inputType))
            {
                int whens = node.WhenClauses.Count;
                for (int i = 0; i < whens; i++)
                {
                    ValidateCanConvertAtoB(node.WhenClauses[i].WhenExpression, inputType);
                }
            }

            var thenExpressions = node.ExtractOutputExpressions().ToArray();
            var resultingType = typeEvaluator.GetExpressionType(thenExpressions);
            if (string.IsNullOrEmpty(resultingType))
            {
                return;
            }

            // all THEN results are compatible with CASE output type
            foreach (var then in thenExpressions)
            {
                // this will evaluate ELSE clause too
                ValidateCanConvertAtoB(then, resultingType);
            }
        }
    }
}
