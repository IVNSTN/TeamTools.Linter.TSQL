using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0451", "UNRESOLVED_WINDOW_NAME")]
    [CompatibilityLevel(SqlVersion.Sql160)]
    internal sealed class UnresolvedWindowNameRule : AbstractRule
    {
        public UnresolvedWindowNameRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            var definedWindows = ExtractWindowNames(node.WindowClause?.WindowDefinition);
            node.AcceptChildren(new OverClauseVisitor(definedWindows, ViolationHandlerWithMessage));
        }

        private static HashSet<string> ExtractWindowNames(IList<WindowDefinition> windows)
        {
            if (windows is null)
            {
                return default;
            }

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = windows.Count - 1; i >= 0; i--)
            {
                result.Add(windows[i].WindowName.Value);
            }

            return result;
        }

        private sealed class OverClauseVisitor : TSqlFragmentVisitor
        {
            private readonly HashSet<string> knownWindows;
            private readonly Action<TSqlFragment, string> callback;

            public OverClauseVisitor(HashSet<string> knownWindows, Action<TSqlFragment, string> callback)
            {
                this.knownWindows = knownWindows;
                this.callback = callback;
            }

            public override void Visit(OverClause node)
            {
                if (node.WindowName != null
                && (knownWindows is null || !knownWindows.Contains(node.WindowName.Value)))
                {
                    callback(node.WindowName, node.WindowName.Value);
                }
            }
        }
    }
}
