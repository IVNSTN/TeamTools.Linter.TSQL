using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    internal abstract class EvaluatorBasedRule<T> : ScriptAnalysisServiceConsumingRule
    where T : SqlViolation
    {
        protected override void ValidateBatch(TSqlBatch node)
        {
            if (!ScalarExpressionEvaluator.IsBatchInteresting(node))
            {
                return;
            }

            var expressionEvaluator = GetService<ScalarExpressionEvaluator>(node);
            ReportViolations(expressionEvaluator.Violations, ViolationHandlerWithMessage);
        }

        private static void ReportViolations(List<SqlViolation> violations, Action<TSqlFragment, string> callback)
        {
            int n = violations.Count;
            for (int i = 0; i < n; i++)
            {
                var v = violations[i];
                if (v.Source?.Node != null && v is T)
                {
                    callback(v.Source.Node, v.Message);
                }
            }
        }
    }
}
