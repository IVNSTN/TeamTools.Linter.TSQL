using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0744", "CURSOR_NOT_MODIFIABLE")]
    [CursorRule]
    internal sealed class UpdatingReadonlyCursorRule : AbstractRule
    {
        public UpdatingReadonlyCursorRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            node.AcceptChildren(new CursorUsageVisitor(ViolationHandlerWithMessage));
        }

        private class CursorUsageVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> callback;
            private readonly Dictionary<string, bool> cursors = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            public CursorUsageVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(DeclareCursorStatement node) => RegisterCursor(node.Name.Value, node.CursorDefinition);

            public override void Visit(SetVariableStatement node) => RegisterCursor(node.Variable.Name, node.CursorDefinition);

            public override void Visit(UpdateSpecification node) => ValidateCursorIsWritable(node.WhereClause?.Cursor);

            public override void Visit(DeleteSpecification node) => ValidateCursorIsWritable(node.WhereClause?.Cursor);

            private static bool IsCursorWritable(CursorDefinition cr)
            {
                // Option combatibility is validated by a separate rule
                if (cr.Select.QueryExpression.ForClause is ReadOnlyForClause)
                {
                    return false;
                }

                if (HasReadonlyOption(cr.Options))
                {
                    return false;
                }

                return true;
            }

            private static bool HasReadonlyOption(IList<CursorOption> options)
            {
                int n = options.Count;
                for (int i = 0; i < n; i++)
                {
                    if (IsReadonlyOption(options[i].OptionKind))
                    {
                        return true;
                    }
                }

                return false;
            }

            private static bool IsReadonlyOption(CursorOptionKind opt)
            {
                return opt == CursorOptionKind.FastForward
                    || opt == CursorOptionKind.ReadOnly
                    || opt == CursorOptionKind.Static;
            }

            private void RegisterCursor(string cursorName, CursorDefinition def)
            {
                if (string.IsNullOrEmpty(cursorName) || def is null)
                {
                    return;
                }

                bool isWritable = IsCursorWritable(def);

                if (!cursors.TryAdd(cursorName, isWritable))
                {
                    // cursor name can be reused
                    cursors[cursorName] = isWritable;
                }
            }

            private void ValidateCursorIsWritable(CursorId cr)
            {
                string cursorName = cr?.Name.Value;
                if (string.IsNullOrEmpty(cursorName) || cursors is null
                || !cursors.TryGetValue(cursorName, out var isWritable) || isWritable)
                {
                    return;
                }

                callback(cr, cursorName);
            }
        }
    }
}
