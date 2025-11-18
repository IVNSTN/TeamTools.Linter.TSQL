using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0946", "LITERAL_OVERSIZE")]
    internal sealed class LiteralOversizeRule : ScriptAnalysisServiceConsumingRule
    {
        public LiteralOversizeRule() : base()
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
                if (v is ImplicitTruncationViolation imp && imp.ValueSource?.Node != null
                && imp.ValueSource.SourceKind == SqlValueSourceKind.Literal)
                {
                    HandleNodeError(imp.ValueSource.Node, v.Message);
                }
            }
        }
    }
}
