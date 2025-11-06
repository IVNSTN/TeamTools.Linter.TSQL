using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0197", "CURSOR_COMMAND_ORDER")]
    [CursorRule]
    internal sealed class CursorCommandOrderRule : AbstractRule
    {
        public CursorCommandOrderRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var cursorCommands = new CursorCommandVisitor(HandleNodeError);

            node.AcceptChildren(cursorCommands);

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

            foreach (var cursor in notDeallocatedCursors)
            {
                string msg = $"Cursor '{cursor}' was not deallocated";

                if (cursorCommands.Declarations.ContainsKey(cursor))
                {
                    HandleNodeError(cursorCommands.Declarations[cursor], msg);
                }
                else
                {
                    HandleLineError(lastToken.Line, lastToken.Column, msg);
                }
            }
        }

        private class CursorCommandVisitor : TSqlFragmentVisitor
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

            public IDictionary<string, CursorCommandType> Commands { get; } =
                new Dictionary<string, CursorCommandType>(StringComparer.OrdinalIgnoreCase);

            public IDictionary<string, TSqlFragment> Declarations { get; } =
                new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

            public override void Visit(DeclareCursorStatement node)
            {
                string cursor = node.Name.Value;
                if (Commands.ContainsKey(cursor)
                && Commands[cursor] != CursorCommandType.Deallocate)
                {
                    callback(node, $"Redeclaring never deallocated cursor '{cursor}'");
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
                if (Commands.ContainsKey(cursor)
                && Commands[cursor] != CursorCommandType.Deallocate)
                {
                    callback(node, $"Redeclaring never deallocated cursor '{cursor}'");
                }

                Commands[cursor] = CursorCommandType.Declare;
                Declarations[cursor] = node.Variable;
            }

            public override void Visit(OpenCursorStatement node)
            {
                string cursor = node.Cursor.Name.Value;
                if (!Commands.ContainsKey(cursor)
                || Commands[cursor] != CursorCommandType.Declare)
                {
                    callback(node, $"Opening not declared or already opened cursor '{cursor}'");
                }

                Commands[cursor] = CursorCommandType.Open;
                if (!Declarations.ContainsKey(cursor))
                {
                    Declarations[cursor] = node.Cursor.Name;
                }
            }

            public override void Visit(FetchCursorStatement node)
            {
                string cursor = node.Cursor.Name.Value;
                if (!Commands.ContainsKey(cursor)
                || !(Commands[cursor] == CursorCommandType.Open || Commands[cursor] == CursorCommandType.Fetch))
                {
                    callback(node, $"Fetching from not opened cursor '{cursor}'");
                }

                Commands[cursor] = CursorCommandType.Fetch;
            }

            public override void Visit(CloseCursorStatement node)
            {
                string cursor = node.Cursor.Name.Value;
                if (!Commands.ContainsKey(cursor)
                || Commands[cursor] != CursorCommandType.Fetch)
                {
                    callback(node, $"Closing cursor '{cursor}' that was never used for fetching");
                }

                Commands[cursor] = CursorCommandType.Close;
            }

            public override void Visit(DeallocateCursorStatement node)
            {
                string cursor = node.Cursor.Name.Value;
                if (!Commands.ContainsKey(cursor)
                || Commands[cursor] != CursorCommandType.Close)
                {
                    callback(node, $"Deallocating unclosed '{cursor}' cursor");
                }

                Commands[cursor] = CursorCommandType.Deallocate;
            }
        }
    }
}
