using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0982", "CURSOR_FORWARD_ONLY")]
    [CursorRule]
    internal sealed class CursorForwardOnlyRule : AbstractRule
    {
        public CursorForwardOnlyRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            var cursorVisitor = new CursorFetchingVisitor();
            node.AcceptChildren(cursorVisitor);

            if (cursorVisitor.NonForwardOnlyCursors is null || cursorVisitor.NonForwardOnlyCursors.Value.Count == 0)
            {
                return;
            }

            foreach (var cr in cursorVisitor.NonForwardOnlyCursors.Value)
            {
                if (!cr.Value.IsScrolled)
                {
                    HandleTokenError(TokenLocator.LocateFirstBeforeOrDefault(cr.Value.Node, TSqlTokenType.Cursor), cr.Key);
                }
            }
        }

        private class FetchCursorInfo
        {
            public CursorDefinition Node { get; set; }

            public bool IsScrolled { get; set; } = false;
        }

        private class CursorFetchingVisitor : TSqlFragmentVisitor
        {
            public CursorFetchingVisitor()
            {
            }

            // cursor name, cursor definition node, is it ever fetched non-forward way
            public Lazy<Dictionary<string, FetchCursorInfo>> NonForwardOnlyCursors { get; } = new Lazy<Dictionary<string, FetchCursorInfo>>(
                () => new Dictionary<string, FetchCursorInfo>(StringComparer.OrdinalIgnoreCase));

            public override void Visit(SetVariableStatement node)
            {
                if (node.CursorDefinition is null)
                {
                    return;
                }

                if (IsForwardOnlyCursor(node.CursorDefinition))
                {
                    // cursor is already forward-only
                    return;
                }

                NonForwardOnlyCursors.Value.TryAdd(node.Variable.Name, new FetchCursorInfo { Node = node.CursorDefinition, IsScrolled = false });
            }

            public override void Visit(DeclareCursorStatement node)
            {
                if (IsForwardOnlyCursor(node.CursorDefinition))
                {
                    // cursor is already forward-only
                    return;
                }

                // TODO : support cursor name reusing
                NonForwardOnlyCursors.Value.TryAdd(node.Name.Value, new FetchCursorInfo { Node = node.CursorDefinition, IsScrolled = false });
            }

            public override void Visit(FetchCursorStatement node)
            {
                string cursorName = node.Cursor.Name.Value ?? (node.Cursor.Name.ValueExpression is VariableReference v ? v.Name : "");

                if (!NonForwardOnlyCursors.Value.TryGetValue(cursorName, out var cursorInfo))
                {
                    // unknown cursor
                    return;
                }

                // FETCH NEXT is the default behavior
                if (node.FetchType is null || node.FetchType.Orientation == FetchOrientation.Next)
                {
                    return;
                }

                cursorInfo.IsScrolled = true;
            }

            private static bool IsForwardOnlyCursor(CursorDefinition node)
            {
                int n = node.Options.Count;
                for (int i = 0; i < n; i++)
                {
                    var opt = node.Options[i];
                    if (opt.OptionKind == CursorOptionKind.FastForward
                    || opt.OptionKind == CursorOptionKind.ForwardOnly)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
