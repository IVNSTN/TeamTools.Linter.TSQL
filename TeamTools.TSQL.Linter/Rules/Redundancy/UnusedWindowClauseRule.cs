using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0452", "UNUSED_WINDOW_CLAUSE")]
    [CompatibilityLevel(SqlVersion.Sql160)]
    internal sealed class UnusedWindowClauseRule : AbstractRule
    {
        public UnusedWindowClauseRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            var definedWindows = ExtractWindowNames(node.WindowClause?.WindowDefinition);

            if (definedWindows is null || definedWindows.Count == 0)
            {
                return;
            }

            RemoveUsedWindows(node.WindowClause.WindowDefinition, definedWindows);
            RemoveUsedWindows(node.SelectElements, definedWindows);

            if (definedWindows.Count == 0)
            {
                return;
            }

            foreach (var w in definedWindows)
            {
                HandleNodeError(w.Value, w.Key);
            }
        }

        private static Dictionary<string, TSqlFragment> ExtractWindowNames(IList<WindowDefinition> windows)
        {
            if (windows is null)
            {
                return default;
            }

            var result = new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

            for (int i = windows.Count - 1; i >= 0; i--)
            {
                var windowName = windows[i].WindowName;
                result.Add(windowName.Value, windowName);
            }

            return result;
        }

        private static void RemoveUsedWindows(IList<WindowDefinition> allWindows, Dictionary<string, TSqlFragment> definedWindows)
        {
            for (int i = allWindows.Count - 1; i >= 0; i--)
            {
                var referencedWindowName = allWindows[i].RefWindowName;
                if (referencedWindowName != null)
                {
                    definedWindows.Remove(referencedWindowName.Value);
                }
            }
        }

        private static void RemoveUsedWindows(IList<SelectElement> selectedItems, Dictionary<string, TSqlFragment> definedWindows)
        {
            var visitor = new OverClauseVisitor(definedWindows);
            for (int i = selectedItems.Count - 1; i >= 0; i--)
            {
                if (selectedItems[i] is SelectScalarExpression selExpr
                && !(selExpr.Expression is Literal || selExpr.Expression is VariableReference))
                {
                    selExpr.Expression.Accept(visitor);
                }
            }
        }

        private sealed class OverClauseVisitor : TSqlFragmentVisitor
        {
            private readonly Dictionary<string, TSqlFragment> definedWindows;

            public OverClauseVisitor(Dictionary<string, TSqlFragment> definedWindows)
            {
                this.definedWindows = definedWindows;
            }

            public override void Visit(OverClause node)
            {
                if (node.WindowName != null)
                {
                    definedWindows.Remove(node.WindowName.Value);
                }
            }
        }
    }
}
