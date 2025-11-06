using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0298", "CURSOR_INFINITE_LOOP")]
    internal sealed class CursorInfiniteLoopRule : AbstractRule
    {
        public CursorInfiniteLoopRule() : base()
        {
        }

        public override void Visit(WhileStatement node)
        {
            var fetchStatusVisitor = new FetchStatusVisitor(HandleNodeError);
            node.Predicate.Accept(fetchStatusVisitor);
        }

        private class FetchStatusVisitor : VisitorWithCallback
        {
            private static readonly string FetchStatusVarName = "@@FETCH_STATUS";

            public FetchStatusVisitor(Action<TSqlFragment> callback) : base(callback)
            {
            }

            public override void Visit(GlobalVariableExpression node)
            {
                if (node.Name.Equals(FetchStatusVarName, StringComparison.OrdinalIgnoreCase))
                {
                    Callback(node);
                }
            }
        }
    }
}
