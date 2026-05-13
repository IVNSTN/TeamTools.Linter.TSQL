using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // Opposite of RecursiveCteMaxRecursionRule
    [RuleIdentity("RD0882", "REDUNDANT_MAX_RECURSION")]
    internal sealed class RedundantMaxRecursionRule : AbstractRule
    {
        public RedundantMaxRecursionRule() : base()
        {
        }

        public override void Visit(StatementWithCtesAndXmlNamespaces node)
        {
            var hint = GetMaxRecursionHint(node.OptimizerHints);
            if (hint is null)
            {
                // no MAXRECURSION
                return;
            }

            if (IsRecursive(node.WithCtesAndXmlNamespaces?.CommonTableExpressions))
            {
                return;
            }

            HandleNodeError(hint);
        }

        private static OptimizerHint GetMaxRecursionHint(IList<OptimizerHint> hints)
        {
            for (int i = hints.Count - 1; i >= 0; i--)
            {
                var hint = hints[i];
                if (hint.HintKind == OptimizerHintKind.MaxRecursion)
                {
                    return hint;
                }
            }

            return default;
        }

        private static bool IsRecursive(CommonTableExpression cte)
        {
            var selfVisitor = new SelfReferenceVisitor(cte.ExpressionName.Value);
            cte.AcceptChildren(selfVisitor);

            return selfVisitor.Detected;
        }

        private static bool IsRecursive(IList<CommonTableExpression> ctes)
        {
            if (ctes is null)
            {
                return false;
            }

            for (int i = ctes.Count - 1; i >= 0; i--)
            {
                if (IsRecursive(ctes[i]))
                {
                    return true;
                }
            }

            return false;
        }

        // Copy from RecursiveCteMaxRecursionRule
        private sealed class SelfReferenceVisitor : TSqlViolationDetector
        {
            private readonly string selfName;

            public SelfReferenceVisitor(string selfName)
            {
                this.selfName = selfName;
            }

            public override void Visit(NamedTableReference node)
            {
                if (node.SchemaObject.SchemaIdentifier is null
                && node.SchemaObject.BaseIdentifier.Value.Equals(selfName, StringComparison.OrdinalIgnoreCase))
                {
                    MarkDetected(node);
                }
            }
        }
    }
}
