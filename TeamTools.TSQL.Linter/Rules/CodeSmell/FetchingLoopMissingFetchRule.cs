using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0954", "LOOP_FETCHSTATUS_MISSING_FETCH")]
    [CursorRule]
    internal sealed class FetchingLoopMissingFetchRule : AbstractRule
    {
        public FetchingLoopMissingFetchRule() : base()
        {
        }

        public override void Visit(WhileStatement node)
        {
            var fetchDetector = new FetchStatusDetector();
            node.Accept(fetchDetector);

            if (fetchDetector.Fetch != null)
            {
                // there is FETCH inside loop
                return;
            }

            if (fetchDetector.FetchStatus != null && fetchDetector.FetchStatus.FirstTokenIndex >= node.Predicate.FirstTokenIndex
            && fetchDetector.FetchStatus.LastTokenIndex <= node.Predicate.LastTokenIndex)
            {
                // FETCH_STATUS is checked in predicate but no FETCH inside loop
                HandleNodeError(fetchDetector.FetchStatus);
            }
        }

        private class FetchStatusDetector : TSqlFragmentVisitor
        {
            private const string FetchStatusGlobalVar = "@@FETCH_STATUS";

            public FetchStatusDetector() : base()
            { }

            public FetchCursorStatement Fetch { get; private set; }

            public GlobalVariableExpression FetchStatus { get; private set; }

            public override void Visit(GlobalVariableExpression node)
            {
                if (node.Name.Equals(FetchStatusGlobalVar, StringComparison.OrdinalIgnoreCase))
                {
                    FetchStatus = node;
                }
            }

            public override void Visit(FetchCursorStatement node) => Fetch = node;
        }
    }
}
