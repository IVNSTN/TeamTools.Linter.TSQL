using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// Simple CASE processing.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        private Action<SimpleCaseExpression> simpleCaseEval;

        // Not an assignment but a possible variable value limit
        // CASE @str WHEN 'a' THEN 'b' END
        public override void Visit(SimpleCaseExpression node)
        {
            walkThrough.Run(node, simpleCaseEval ?? (simpleCaseEval = new Action<SimpleCaseExpression>(SimpleCaseEval)));
        }

        private void SimpleCaseEval(SimpleCaseExpression node)
        {
            int n = node.WhenClauses.Count;
            for (int i = 0; i < n; i++)
            {
                var when = node.WhenClauses[i];
                bool limited = conditionHandler.DetectEqualityLimitingVarValues(node.InputExpression, when.WhenExpression);

                when.ThenExpression.Accept(this);

                if (limited)
                {
                    conditionHandler.ResetValueEstimatesAfterConditionalBlock(when);
                }
            }
        }
    }
}
