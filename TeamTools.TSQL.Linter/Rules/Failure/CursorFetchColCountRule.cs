using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0124", "CURSOR_FETCH_COL_COUNT")]
    internal sealed class CursorFetchColCountRule : AbstractRule
    {
        public CursorFetchColCountRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node) => node.Accept(new CursorFetchVisitor(ViolationHandler));

        private sealed class CursorFetchVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment> callback;

            public CursorFetchVisitor(Action<TSqlFragment> callback)
            {
                this.callback = callback;
            }

            public IDictionary<string, int> Cursors { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            public override void Visit(FetchCursorStatement node)
            {
                if (node.IntoVariables is null || node.IntoVariables.Count == 0)
                {
                    // fetch does not have INTO part
                    return;
                }

                string cursorName = node.Cursor.Name.Value;

                if (!Cursors.TryGetValue(cursorName, out var cursorVariables))
                {
                    // never seen this cursor declaration before
                    return;
                }

                if (node.IntoVariables.Count == cursorVariables)
                {
                    // fetching into correct number of vars
                    return;
                }

                callback(node.Cursor.Name);
            }

            public override void Visit(DeclareCursorStatement node)
                => RegisterCursorDefinition(node.Name.Value, node.CursorDefinition);

            public override void Visit(SetVariableStatement node)
                => RegisterCursorDefinition(node.Variable.Name, node.CursorDefinition);

            private static int GetQueryExpressionColCount(QueryExpression node)
                => node.GetQuerySpecification()?.SelectElements.Count ?? 0;

            private static int GetCursorColCount(CursorDefinition node)
                => GetQueryExpressionColCount(node.Select.QueryExpression);

            private void RegisterCursorDefinition(string cursorName, CursorDefinition def)
            {
                if (def is null)
                {
                    return;
                }

                int colCount = GetCursorColCount(def);

                if (!Cursors.TryAdd(cursorName, colCount))
                {
                    // cursor was redeclared
                    Cursors[cursorName] = colCount;
                }
            }
        }
    }
}
