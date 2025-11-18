using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0930", "NAME_REUSED_CURSOR")]
    [CursorRule]
    internal sealed class ReusedCursorNameRule : AbstractRule
    {
        public ReusedCursorNameRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            var visitor = new CursorNameVisitor(ViolationHandlerWithMessage);
            node.AcceptChildren(visitor);
        }

        private class CursorNameVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> callback;
            private HashSet<string> cursorNames;

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

                if (cursorNames is null)
                {
                    cursorNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }

                if (!cursorNames.Add(cursorName))
                {
                    callback(node, cursorName);
                }
            }
        }
    }
}
