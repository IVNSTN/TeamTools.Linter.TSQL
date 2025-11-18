using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0947", "LITERAL_OUT_OF_RANGE")]
    internal sealed class LiteralOutOfRangeRule : ScriptAnalysisServiceConsumingRule
    {
        public LiteralOutOfRangeRule() : base()
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
                if (v is OutOfRangeViolation outOfRange && outOfRange.ValueSource?.Node != null
                && outOfRange.ValueSource.SourceKind == SqlValueSourceKind.Literal)
                {
                    HandleNodeError(outOfRange.ValueSource.Node, v.Message);
                }
            }
        }
    }
}
