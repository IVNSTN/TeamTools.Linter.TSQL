using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// Simple CASE processing.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        // Not an assignment but a possible variable value limit
        // CASE @str WHEN 'a' THEN 'b' END
        public override void Visit(SimpleCaseExpression node)
        {
            walkThrough.Run(node, () =>
            {
                foreach (var when in node.WhenClauses)
                {
                    bool limited = conditionHandler.DetectEqualityLimitingVarValues(node.InputExpression, when.WhenExpression);

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
