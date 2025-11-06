using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("MA0174", "CLOCK_BASED_CODE_FLOW")]
    internal sealed class ClockBasedCodeFlowRule : AbstractRule
    {
        public ClockBasedCodeFlowRule() : base()
        {
        }

        public override void Visit(IfStatement node) => DetectClockBasedCode(node.Predicate);

        public override void Visit(WhereClause node) => DetectClockBasedCode(node.SearchCondition);

        public override void Visit(WhileStatement node) => DetectClockBasedCode(node.Predicate);

        private void DetectClockBasedCode(BooleanExpression node)
            => TSqlViolationDetector.DetectFirst<ClockVisitor>(node, HandleNodeError);

        private class ClockVisitor : TSqlViolationDetector
        {
            private static readonly ICollection<string> ClockFunctions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "GETDATE",
                "GETUTCDATE",
                "SYSDATETIME",
                "SYSUTCDATETIME",
                "SYSDATETIMEOFFSET",
            };

            public override void Visit(FunctionCall node)
            {
                if (ClockFunctions.Contains(node.FunctionName.Value))
                {
                    MarkDetected(node);
                }
            }
        }
    }
}
