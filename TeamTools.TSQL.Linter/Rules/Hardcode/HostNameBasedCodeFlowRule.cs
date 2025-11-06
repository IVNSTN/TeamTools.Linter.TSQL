using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("HD0149", "HOST_NAME_BASED_FLOW")]
    internal sealed class HostNameBasedCodeFlowRule : AbstractRule
    {
        public HostNameBasedCodeFlowRule() : base()
        {
        }

        public override void Visit(IfStatement node) => ValidatePredicate(node.Predicate);

        public override void Visit(WhereClause node) => ValidatePredicate(node.SearchCondition);

        public override void Visit(WhileStatement node) => ValidatePredicate(node.Predicate);

        private void ValidatePredicate(BooleanExpression node)
            => TSqlViolationDetector.DetectFirst<HostNameVisitor>(node, HandleNodeError);

        private class HostNameVisitor : TSqlViolationDetector
        {
            public override void Visit(FunctionCall node)
            {
                if (node.FunctionName.Value.Equals("HOST_NAME", StringComparison.OrdinalIgnoreCase))
                {
                    MarkDetected(node);
                }
            }
        }
    }
}
