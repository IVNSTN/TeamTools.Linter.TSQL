using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    public static class ScriptDomIteratorExtension
    {
        public static IEnumerable<TSqlFragment> EnumElements(this TSqlFragment node)
        {
            var el = new List<TSqlFragment>();

            node.AcceptChildren(new AllNodeVisitor(nd => el.Add(nd)));

            return el;
        }

        private sealed class AllNodeVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment> callback;

            public AllNodeVisitor(Action<TSqlFragment> callback)
            {
                this.callback = callback;
            }

            public override void Visit(TSqlFragment node)
            {
                callback.Invoke(node);
            }
        }
    }
}
