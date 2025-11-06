using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0930", "NAME_REUSED_CURSOR")]
    internal sealed class ReusedCursorNameRule : AbstractRule
    {
        public ReusedCursorNameRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var visitor = new CursorNameVisitor(HandleNodeError);
            node.AcceptChildren(visitor);
        }

        private class CursorNameVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> callback;
            private readonly ICollection<string> cursorNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            public CursorNameVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(DeclareCursorStatement node)
            {
                var cursorName = node.Name?.Value;
                if (cursorName is null)
                {
                    return;
                }

                if (!cursorNames.TryAddUnique(cursorName))
                {
                    callback(node, cursorName);
                }
            }
        }
    }
}
