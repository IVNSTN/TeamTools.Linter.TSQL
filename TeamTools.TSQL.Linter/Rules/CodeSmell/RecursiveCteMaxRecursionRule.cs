using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0942", "CTE_MAX_RECURSION")]
    internal sealed class RecursiveCteMaxRecursionRule : AbstractRule
    {
        public RecursiveCteMaxRecursionRule() : base()
        {
        }

        public override void Visit(StatementWithCtesAndXmlNamespaces node)
        {
            if ((node.WithCtesAndXmlNamespaces?.CommonTableExpressions.Count ?? 0) == 0)
            {
                return;
            }

            if (node.OptimizerHints.HasHint(OptimizerHintKind.MaxRecursion))
            {
                return;
            }

            ValidateCtes(node.WithCtesAndXmlNamespaces.CommonTableExpressions);
        }

        private static bool IsRecursive(CommonTableExpression cte)
        {
            var selfVisitor = new SelfReferenceVisitor(cte.ExpressionName.Value);
            cte.AcceptChildren(selfVisitor);

            return selfVisitor.Detected;
        }

        private void ValidateCtes(IList<CommonTableExpression> ctes)
        {
            for (int i = 0, n = ctes.Count; i < n; i++)
            {
                var cte = ctes[i];
                if (IsRecursive(cte))
                {
                    HandleNodeError(cte.ExpressionName);
                    return;
                }
            }
        }

        private class SelfReferenceVisitor : TSqlViolationDetector
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
