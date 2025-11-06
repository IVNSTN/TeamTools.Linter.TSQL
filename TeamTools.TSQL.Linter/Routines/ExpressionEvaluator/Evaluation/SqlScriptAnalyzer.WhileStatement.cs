using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// While statement analysis.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        // Not an assignment but a possible variable value limit
        // TODO : not sure because variable may be rewritten inside loop many times
        public override void Visit(WhileStatement node)
        {
            walkThrough.Run(node, () =>
            {
                // TODO : this may lead to dup violation registration
                // but DetectPredicatesLimitingVarValues does not evaluate everithing
                node.Predicate.Accept(this);

                conditionHandler.DetectPredicatesLimitingVarValues(node.Predicate);

                node.Statement.Accept(this);

                conditionHandler.ResetValueEstimatesAfterConditionalBlock(node);
            });
        }
    }
}
