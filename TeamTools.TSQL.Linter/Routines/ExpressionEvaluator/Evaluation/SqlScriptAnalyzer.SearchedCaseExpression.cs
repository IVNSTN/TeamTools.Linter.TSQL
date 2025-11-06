using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// Searched CASE processing.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        // Not an assignment but a possible variable value limit
        // CASE WHEN @str = 'a' THEN 'b' END
        public override void Visit(SearchedCaseExpression node)
        {
            walkThrough.Run(node, () =>
            {
                foreach (var when in node.WhenClauses)
                {
                    bool limited = conditionHandler.DetectPredicatesLimitingVarValues(when.WhenExpression);

                    when.ThenExpression.Accept(this);

                    if (limited)
                    {
                        conditionHandler.ResetValueEstimatesAfterConditionalBlock(when);
                    }
                }
            });
        }
    }
}
