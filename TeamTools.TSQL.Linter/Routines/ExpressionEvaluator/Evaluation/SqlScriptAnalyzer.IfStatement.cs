using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// IF statements processing. Predicate can limit var value for the THEN block.
    /// e.g. IF @var &gt; 0 and @var &lt; 100 means that if we got into THEN block
    /// then @var is in (0, 100) range for sure.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        // Not an assignment but a possible variable value limit
        // TODO : handle ELSE IF, ELSE
        public override void Visit(IfStatement node)
        {
            walkThrough.Run(node.ThenStatement, () =>
            {
                ProcessIfElse(node);
            });
        }

        private void ProcessIfElse(IfStatement node)
        {
            // TODO : this may lead to dup violation registration
            // but DetectPredicatesLimitingVarValues does not evaluate everithing
            node.Predicate.Accept(this);

            bool limited = ConditionHandler.DetectPredicatesLimitingVarValues(node.Predicate);

            node.ThenStatement.Accept(this);

            // Not node.ThenStatement because if var limit was detected in node.Predicate
            // we need to reset it as well. Also after all the if-then-if-then-else
            // we'd still reset each variable assignment thus is okay to reset everytime
            // for the outermost IF node. We have already analyzed THEN block in the line above.
            // FIXME : I don't understand this
            if (limited)
            {
                ConditionHandler.ResetValueEstimatesAfterConditionalBlock(node.Predicate);
            }

            bool mergeAllOptions = node.ElseStatement != null;

            if (this.CheckIfBlockBreaksBatch(node.ThenStatement))
            {
                ConditionHandler.RevertValueEstimatesToBeforeBlock(node.ThenStatement);
            }
            else if (!this.CheckIfBlockIsNestedIf(node.ThenStatement))
            {
                ConditionHandler.ResetValueEstimatesAfterConditionalBlock(node.ThenStatement);
            }

            if (node.ElseStatement != null)
            {
                node.ElseStatement.Accept(this);

                if (!this.CheckIfBlockIsNestedIf(node.ElseStatement)
                && this.CheckIfBlockBreaksBatch(node.ElseStatement))
                {
                    ConditionHandler.RevertValueEstimatesToBeforeBlock(node.ElseStatement);
                    mergeAllOptions = false;
                }
            }

            if (mergeAllOptions)
            {
                ConditionHandler.ResetValueEstimatesAfterConditionalBlock(node);
            }
        }
    }
}
