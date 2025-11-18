using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0197", "CURSOR_COMMAND_ORDER")]
    [CursorRule]
    internal sealed class CursorCommandOrderRule : AbstractRule
    {
        public CursorCommandOrderRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            var cursorCommands = new CursorCommandVisitor(ViolationHandlerWithMessage);

            node.AcceptChildren(cursorCommands);

            if (cursorCommands.Commands.Count == 0)
            {
                return;
            }

            // In the end all cursors must be deallocated
            var notDeallocatedCursors = cursorCommands.Commands
                .Where(cr => cr.Value != CursorCommandVisitor.CursorCommandType.Deallocate)
                .Select(cr => cr.Key)
                .ToList();

            if (notDeallocatedCursors.Count == 0)
            {
                return;
            }

            var lastToken = node.ScriptTokenStream[node.LastTokenIndex];

            int n = notDeallocatedCursors.Count;
            for (int i = 0; i < n; i++)
            {
                var cursor = notDeallocatedCursors[i];

                string msg = string.Format(Strings.ViolationDetails_CursorCommandOrderRule_NotDeallocated, cursor);

                if (cursorCommands.Declarations.TryGetValue(cursor, out var cursorLocation))
                {
                    HandleNodeError(cursorLocation, msg);
                }
                else
                {
                    HandleLineError(lastToken.Line, lastToken.Column, msg);
                }
            }
        }

        private sealed class CursorCommandVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> callback;

            public CursorCommandVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public enum CursorCommandType
            {
                Declare,
                Open,
                Fetch,
                Close,
                Deallocate,
            }

            public Dictionary<string, CursorCommandType> Commands { get; } =
                new Dictionary<string, CursorCommandType>(StringComparer.OrdinalIgnoreCase);

            public Dictionary<string, TSqlFragment> Declarations { get; } =
                new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

            public override void Visit(DeclareCursorStatement node)
            {
                string cursor = node.Name.Value;
                if (Commands.TryGetValue(cursor, out CursorCommandType cursorState)
                && cursorState != CursorCommandType.Deallocate)
                {
                    callback(node, string.Format(Strings.ViolationDetails_CursorCommandOrderRule_RedeclaringNotDeallocated, cursor));
                }

                Commands[cursor] = CursorCommandType.Declare;
                Declarations[cursor] = node.Name;
            }

            public override void Visit(SetVariableStatement node)
            {
                if (node.CursorDefinition is null)
                {
                    return;
                }

                string cursor = node.Variable.Name;
                if (Commands.TryGetValue(cursor, out CursorCommandType cursorState)
                && cursorState != CursorCommandType.Deallocate)
                {
                    callback(node, string.Format(Strings.ViolationDetails_CursorCommandOrderRule_RedeclaringNotDeallocated, cursor));
                }

                Commands[cursor] = CursorCommandType.Declare;
                Declarations[cursor] = node.Variable;
            }

            public override void Visit(OpenCursorStatement node)
            {
                string cursor = node.Cursor.Name.Value;
                if (!Commands.TryGetValue(cursor, out CursorCommandType cursorState)
                || cursorState != CursorCommandType.Declare)
                {
                    callback(node, string.Format(Strings.ViolationDetails_CursorCommandOrderRule_OpeningNotDeclared, cursor));
                }

                Commands[cursor] = CursorCommandType.Open;
                Declarations.TryAdd(cursor, node.Cursor.Name);
            }

            public override void Visit(FetchCursorStatement node)
            {
                string cursor = node.Cursor.Name.Value;
                if (!Commands.TryGetValue(cursor, out CursorCommandType cursorState)
                || !(cursorState == CursorCommandType.Open || cursorState == CursorCommandType.Fetch))
                {
                    callback(node, string.Format(Strings.ViolationDetails_CursorCommandOrderRule_FetchingFromNotOpened, cursor));
                }

                Commands[cursor] = CursorCommandType.Fetch;
            }

            public override void Visit(CloseCursorStatement node)
            {
                string cursor = node.Cursor.Name.Value;
                if (!Commands.TryGetValue(cursor, out CursorCommandType cursorState)
                || cursorState != CursorCommandType.Fetch)
                {
                    callback(node, string.Format(Strings.ViolationDetails_CursorCommandOrderRule_ClosingNeverFetched, cursor));
                }

                Commands[cursor] = CursorCommandType.Close;
            }

            public override void Visit(DeallocateCursorStatement node)
            {
                string cursor = node.Cursor.Name.Value;
                if (!Commands.TryGetValue(cursor, out CursorCommandType cursorState)
                || cursorState != CursorCommandType.Close)
                {
                    callback(node, string.Format(Strings.ViolationDetails_CursorCommandOrderRule_DeallocatingUnclosed, cursor));
                }

                Commands[cursor] = CursorCommandType.Deallocate;
            }
        }
    }
}
