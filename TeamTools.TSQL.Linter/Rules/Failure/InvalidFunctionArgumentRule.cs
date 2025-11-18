using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0705", "INVALID_ARGUMENT")]
    internal sealed class InvalidFunctionArgumentRule : ScriptAnalysisServiceConsumingRule
    {
        public InvalidFunctionArgumentRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            if (!ScalarExpressionEvaluator.IsBatchInteresting(node))
            {
                return;
            }

            var expressionEvaluator = GetService<ScalarExpressionEvaluator>(node);

            int n = expressionEvaluator.Violations.Count;
            for (int i = 0; i < n; i++)
            {
                var v = expressionEvaluator.Violations[i];
                if (v.Source?.Node != null
                && (v is InvalidArgumentViolation || v is ArgumentOutOfRangeViolation))
                {
                    HandleNodeError(v.Source.Node, v.Message);
                }
            }
        }
    }
}
