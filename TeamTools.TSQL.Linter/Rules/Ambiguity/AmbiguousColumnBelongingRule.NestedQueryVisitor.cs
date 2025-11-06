using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Nested query visitor.
    /// </summary>
    internal partial class AmbiguousColumnBelongingRule
    {
        private class NestedQueryVisitor : TSqlFragmentVisitor
        {
            private readonly Action<QuerySpecification> callback;
            private readonly bool detectOnlyWithFrom;

            public NestedQueryVisitor(Action<QuerySpecification> callback, bool detectOnlyWithFrom)
            {
                this.callback = callback;
                this.detectOnlyWithFrom = detectOnlyWithFrom;
            }

            public override void Visit(QuerySpecification node)
            {
                if (detectOnlyWithFrom && node.FromClause is null)
                {
                    return;
                }

                callback(node);
            }
        }
    }
}
