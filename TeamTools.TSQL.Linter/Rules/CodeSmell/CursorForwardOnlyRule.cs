using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0982", "CURSOR_FORWARD_ONLY")]
    [CursorRule]
    internal sealed class CursorForwardOnlyRule : AbstractRule
    {
        private static readonly ICollection<CursorOptionKind> ForwardCursorOptions = new List<CursorOptionKind>
        {
            CursorOptionKind.FastForward,
            CursorOptionKind.ForwardOnly,
        };

        public CursorForwardOnlyRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var cursorVisitor = new CursorFetchingVisitor();
            node.AcceptChildren(cursorVisitor);

            if (!cursorVisitor.NonForwardOnlyCursors.Any())
            {
                return;
            }

            foreach (var cr in cursorVisitor.NonForwardOnlyCursors.Where(c => !c.Value.IsScrolled))
            {
                HandleTokenError(TokenLocator.LocateFirstBeforeOrDefault(cr.Value.Node, TSqlTokenType.Cursor), cr.Key);
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
            public IDictionary<string, FetchCursorInfo> NonForwardOnlyCursors { get; } = new Dictionary<string, FetchCursorInfo>(StringComparer.OrdinalIgnoreCase);

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

                NonForwardOnlyCursors.TryAdd(node.Variable.Name, new FetchCursorInfo { Node = node.CursorDefinition, IsScrolled = false });
            }

            public override void Visit(DeclareCursorStatement node)
            {
                if (IsForwardOnlyCursor(node.CursorDefinition))
                {
                    // cursor is already forward-only
                    return;
                }

                // TODO : support cursor name reusing
                NonForwardOnlyCursors.TryAdd(node.Name.Value, new FetchCursorInfo { Node = node.CursorDefinition, IsScrolled = false });
            }

            public override void Visit(FetchCursorStatement node)
            {
                string cursorName = node.Cursor.Name.Value ?? (node.Cursor.Name.ValueExpression is VariableReference v ? v.Name : "");

                if (!NonForwardOnlyCursors.ContainsKey(cursorName))
                {
                    // unknown cursor
                    return;
                }

                // FETCH NEXT is the default behavior
                if (node.FetchType is null || node.FetchType.Orientation == FetchOrientation.Next)
                {
                    return;
                }

                NonForwardOnlyCursors[cursorName].IsScrolled = true;
            }

            private static bool IsForwardOnlyCursor(CursorDefinition node)
                => node.Options.Any(opt => ForwardCursorOptions.Contains(opt.OptionKind));
        }
    }
}
