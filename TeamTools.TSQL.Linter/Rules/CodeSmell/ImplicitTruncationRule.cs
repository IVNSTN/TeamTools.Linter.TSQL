using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0937", "IMPLICIT_STRING_TRUNCATION")]
    internal class ImplicitTruncationRule : ScriptAnalysisServiceConsumingRule
    {
        public ImplicitTruncationRule() : base()
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
                && v is ImplicitTruncationViolation imp && imp.ValueSource?.Node != null)
                {
                    HandleNodeError(v.Source.Node, v.Message);
                }
            }
        }
    }
}
