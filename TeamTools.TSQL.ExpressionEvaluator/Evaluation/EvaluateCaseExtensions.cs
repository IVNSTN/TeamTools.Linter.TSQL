using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.Core;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    // TODO : combine somehow with VariableAssignmentVisitor methods analyzing CASE expressions too
    public static class EvaluateCaseExtensions
    {
        // TODO : respect always true/false predicates
        public static SqlValue EvaluateCaseExpression(this SqlExpressionEvaluator eval, SearchedCaseExpression expr)
        {
            List<SqlValue> thenResults = new List<SqlValue>(expr.WhenClauses.Count + 1);

            int n = expr.WhenClauses.Count;
            for (int i = 0; i < n; i++)
            {
                var when = expr.WhenClauses[i];
                if (eval.ConditionHander.DetectPredicatesLimitingVarValues(when.WhenExpression))
                {
                    eval.ConditionHander.ResetValueEstimatesAfterConditionalBlock(when);
                }

                thenResults.Add(eval.EvaluateExpression(when.ThenExpression));
            }

            if (expr.ElseExpression != null)
            {
                thenResults.Add(eval.EvaluateExpression(expr.ElseExpression));
            }

            return DoEvaluateCaseExpressionResult(eval, thenResults);
        }

        // TODO : respect always true/false predicates and WHEN values compatibility
        public static SqlValue EvaluateCaseExpression(this SqlExpressionEvaluator eval, SimpleCaseExpression expr)
        {
            List<SqlValue> thenResults = new List<SqlValue>(expr.WhenClauses.Count + 1);

            int n = expr.WhenClauses.Count;
            for (int i = 0; i < n; i++)
            {
                var when = expr.WhenClauses[i];
                if (eval.ConditionHander.DetectEqualityLimitingVarValues(expr.InputExpression, when.WhenExpression))
                {
                    eval.ConditionHander.ResetValueEstimatesAfterConditionalBlock(when);
                }

                thenResults.Add(eval.EvaluateExpression(when.ThenExpression));
            }

            if (expr.ElseExpression != null)
            {
                thenResults.Add(eval.EvaluateExpression(expr.ElseExpression));
            }

            return DoEvaluateCaseExpressionResult(eval, thenResults);
        }

        // TODO : this is very similar to CaseBasedResultFunctionHandler
        // move to surrogate function evaluator?
        private static SqlValue DoEvaluateCaseExpressionResult(SqlExpressionEvaluator eval, List<SqlValue> thenResults)
        {
            if (thenResults.Any(v => v is null))
            {
                // some THEN results could not be evaluated
                // output type is unpredictable or unsupported
                return default;
            }

            var outputType = eval.Converter.EvaluateOutputType(thenResults);
            if (string.IsNullOrEmpty(outputType))
            {
                return default;
            }

            SqlValue result = eval.Converter.ImplicitlyConvertTo(outputType, thenResults[0]);
            if (result is null)
            {
                return default;
            }

            var resultTypeHandler = result.GetTypeHandler();

            for (int i = 1, n = thenResults.Count; i < n; i++)
            {
                result = resultTypeHandler.MergeTwoEstimates(result, thenResults[i]);
                if (result is null)
                {
                    // If result becomes null it means we are unable to continue estimation
                    return default;
                }
            }

            return eval.Converter.ImplicitlyConvertTo(outputType, result);
        }
    }
}
