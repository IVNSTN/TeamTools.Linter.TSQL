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

        public override void Visit(TSqlBatch node)
        {
            var cursorVisitor = new CursorDeclarationVisitor();
            node.AcceptChildren(cursorVisitor);
            var fetchVisitor = new CursorFetchVisitor(cursorVisitor.Cursors, HandleNodeError);
            node.AcceptChildren(fetchVisitor);
        }

        private readonly struct CursorDeclaration
        {
            public readonly int DefinitionLine;
            public readonly int ColumnCount;

            public CursorDeclaration(int startLine, int colCount)
            {
                DefinitionLine = startLine;
                ColumnCount = colCount;
            }
        }

        private class CursorFetchVisitor : TSqlFragmentVisitor
        {
            private readonly IDictionary<string, IList<CursorDeclaration>> cursorDeclarations;
            private readonly Action<TSqlFragment> callback;

            public CursorFetchVisitor(IDictionary<string, IList<CursorDeclaration>> cursorDeclarations, Action<TSqlFragment> callback)
            {
                this.cursorDeclarations = cursorDeclarations;
                this.callback = callback;
            }

            public override void Visit(FetchCursorStatement node)
            {
                string cursorName = node.Cursor.Name.Value;

                if (!cursorDeclarations.ContainsKey(cursorName))
                {
                    return;
                }

                if ((node.IntoVariables == null) || (node.IntoVariables.Count == 0))
                {
                    return;
                }

                // TODO : refactor
                var cursor = cursorDeclarations[cursorName];
                int matchingDeclarationIndex = -1;
                int n = cursor.Count;
                for (int i = 0; i < n; i++)
                {
                    if (cursor[i].DefinitionLine <= node.StartLine)
                    {
                        if (matchingDeclarationIndex == -1)
                        {
                            matchingDeclarationIndex = i;
                        }
                        else if (cursor[i].DefinitionLine > cursor[matchingDeclarationIndex].DefinitionLine)
                        {
                            matchingDeclarationIndex = i;
                        }
                    }
                }

                if (node.IntoVariables.Count == cursor[matchingDeclarationIndex].ColumnCount)
                {
                    return;
                }

                callback(node);
            }
        }

        private class CursorDeclarationVisitor : TSqlFragmentVisitor
        {
            public CursorDeclarationVisitor()
            {
            }

            public IDictionary<string, IList<CursorDeclaration>> Cursors { get; } = new SortedDictionary<string, IList<CursorDeclaration>>(StringComparer.OrdinalIgnoreCase);

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

                if (!Cursors.ContainsKey(cursorName))
                {
                    Cursors.Add(cursorName, new List<CursorDeclaration>());
                }

                Cursors[cursorName].Add(new CursorDeclaration(def.StartLine, colCount));
            }
        }
    }
}
