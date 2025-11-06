using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void Visit(TSqlBatch node)
        {
            node.AcceptChildren(new CursorUsageVisitor(HandleNodeError));
        }

        private class CursorUsageVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> callback;
            private readonly IDictionary<string, bool> cursors = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

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
                if (cr.Options.Any(opt => opt.OptionKind.In(CursorOptionKind.FastForward, CursorOptionKind.ReadOnly, CursorOptionKind.Static)))
                {
                    return false;
                }

                // Option combatibility is validated by a separate rule
                if (cr.Select.QueryExpression.ForClause is ReadOnlyForClause)
                {
                    return false;
                }

                return true;
            }

            private void RegisterCursor(string cursorName, CursorDefinition def)
            {
                if (string.IsNullOrEmpty(cursorName) || def is null)
                {
                    return;
                }

                bool isWritable = IsCursorWritable(def);

                if (cursors.ContainsKey(cursorName))
                {
                    // cursor name can be reused
                    cursors[cursorName] = isWritable;
                }
                else
                {
                    cursors.Add(cursorName, isWritable);
                }
            }

            private void ValidateCursorIsWritable(CursorId cr)
            {
                string cursorName = cr?.Name.Value;
                if (string.IsNullOrEmpty(cursorName) || !cursors.ContainsKey(cursorName) || cursors[cr.Name.Value])
                {
                    return;
                }

                callback(cr, cursorName);
            }
        }
    }
}
