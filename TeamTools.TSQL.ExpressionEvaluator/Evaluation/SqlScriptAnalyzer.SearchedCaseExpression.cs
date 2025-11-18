using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// Searched CASE processing.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        private Action<SearchedCaseExpression> searchCaseEval;

        // Not an assignment but a possible variable value limit
        // CASE WHEN @str = 'a' THEN 'b' END
        public override void Visit(SearchedCaseExpression node)
        {
            walkThrough.Run(node, searchCaseEval ?? (searchCaseEval = new Action<SearchedCaseExpression>(SearchCaseEval)));
        }

        private void SearchCaseEval(SearchedCaseExpression node)
        {
            int n = node.WhenClauses.Count;
            for (int i = 0; i < n; i++)
            {
                var when = node.WhenClauses[i];
                bool limited = conditionHandler.DetectPredicatesLimitingVarValues(when.WhenExpression);

                when.ThenExpression.Accept(this);

                if (limited)
                {
                    conditionHandler.ResetValueEstimatesAfterConditionalBlock(when);
                }
            }
        }
    }
}
