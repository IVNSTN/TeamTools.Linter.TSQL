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
        private readonly ClockVisitor visitor;

        public ClockBasedCodeFlowRule() : base()
        {
            visitor = new ClockVisitor(ViolationHandler);
        }

        public override void Visit(IfStatement node) => DetectClockBasedCode(node.Predicate);

        // Note SearchCondition may be null in case of WHERE CURRENT OF
        public override void Visit(WhereClause node) => DetectClockBasedCode(node.SearchCondition);

        public override void Visit(WhileStatement node) => DetectClockBasedCode(node.Predicate);

        private void DetectClockBasedCode(BooleanExpression node) => node?.Accept(visitor);

        private sealed class ClockVisitor : VisitorWithCallback
        {
            private static readonly HashSet<string> ClockFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "GETDATE",
                "GETUTCDATE",
                "SYSDATETIME",
                "SYSUTCDATETIME",
                "SYSDATETIMEOFFSET",
            };

            public ClockVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(FunctionCall node)
            {
                if (ClockFunctions.Contains(node.FunctionName.Value))
                {
                    Callback(node);
                }
            }
        }
    }
}
