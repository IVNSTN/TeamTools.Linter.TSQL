using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0954", "LOOP_FETCHSTATUS_MISSING_FETCH")]
    internal sealed class FetchingLoopMissingFetchRule : AbstractRule
    {
        public FetchingLoopMissingFetchRule() : base()
        {
        }

        public override void Visit(WhileStatement node)
        {
            var fetchDetector = new FetchDetector();
            node.Statement.Accept(fetchDetector);

            if (fetchDetector.Detected)
            {
                return;
            }

            TSqlViolationDetector.DetectFirst<FetchStatusDetector>(node.Predicate, HandleNodeError);
        }

        private class FetchDetector : TSqlViolationDetector
        {
            public override void Visit(FetchCursorStatement node) => MarkDetected(node);
        }

        private class FetchStatusDetector : TSqlViolationDetector
        {
            private const string FetchStatusGlobalVar = "@@FETCH_STATUS";

            public override void Visit(GlobalVariableExpression node)
            {
                if (node.Name.Equals(FetchStatusGlobalVar, StringComparison.OrdinalIgnoreCase))
                {
                    MarkDetected(node);
                }
            }
        }
    }
}
