using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0983", "CURSOR_FETCHED_VAR_UNUSED")]
    [CursorRule]
    internal sealed class CursorFetchedVarUnusedRule : AbstractRule
    {
        public CursorFetchedVarUnusedRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var usageVisitor = new CursorUsageVisitor();
            node.AcceptChildren(usageVisitor);

            if (!usageVisitor.FetchedButNeverUsed.Any())
            {
                return;
            }

            foreach (var unused in usageVisitor.FetchedButNeverUsed.OrderBy(v => v.Key))
            {
                HandleNodeError(unused.Value, unused.Key);
            }
        }

        private class CursorUsageVisitor : TSqlFragmentVisitor
        {
            private readonly List<VariableReference> ignoredVarRefs = new List<VariableReference>();

            private readonly Dictionary<string, ICollection<string>> cursorVars =
                new Dictionary<string, ICollection<string>>(StringComparer.OrdinalIgnoreCase);

            private readonly Dictionary<string, TSqlFragment> fetchedVars =
                new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

            private readonly Dictionary<string, TSqlFragment> unusedVars =
                new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

            public IDictionary<string, TSqlFragment> FetchedButNeverUsed => unusedVars;

            public override void Visit(FetchCursorStatement node)
            {
                foreach (var v in node.IntoVariables)
                {
                    // saving fetch var refs to prevent treating it as variable usage later
                    ignoredVarRefs.Add(v);
                }

                string cursorName = node.Cursor.Name.Value;
                if (string.IsNullOrEmpty(cursorName))
                {
                    return;
                }

                if (!cursorVars.ContainsKey(cursorName))
                {
                    cursorVars.Add(cursorName, new SortedSet<string>());
                }
                else
                {
                    // currently not supporting multiple fetches because they may occur inside loop
                    // and to realize whether fetched value is used or not we should go back and review
                    // loop code. which is not easy.
                    // TODO : support fetches inside loops, multiple fetches from the same cursor
                    return;
                }

                foreach (var varName in node.IntoVariables.Select(vv => vv.Name).Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (!cursorVars[cursorName].Contains(varName))
                    {
                        cursorVars[cursorName].Add(varName);
                    }

                    fetchedVars.TryAdd(varName, node);
                }
            }

            public override void Visit(SelectSetVariable node)
            {
                if (node.AssignmentKind != AssignmentKind.Equals)
                {
                    // self modification means prior value usage
                    return;
                }

                if (node.Expression is null)
                {
                    return;
                }

                // if the variable is referenced in the expression then it is used
                node.Expression.Accept(this);
                // if still exists in fetched (but not used) variables - registering it as unused
                if (fetchedVars.ContainsKey(node.Variable.Name))
                {
                    unusedVars.TryAdd(node.Variable.Name, fetchedVars[node.Variable.Name]);
                }
            }

            public override void Visit(SetVariableStatement node)
            {
                if (node.AssignmentKind != AssignmentKind.Equals)
                {
                    // self modification means prior value usage
                    return;
                }

                if (node.Expression is null)
                {
                    return;
                }

                // if the variable is referenced in the expression then it is used
                node.Expression.Accept(this);
                // if still exists in fetched (but not used) variables - registering it as unused
                if (fetchedVars.ContainsKey(node.Variable.Name))
                {
                    unusedVars.TryAdd(node.Variable.Name, fetchedVars[node.Variable.Name]);
                }
            }

            public override void Visit(VariableReference node)
            {
                if (ignoredVarRefs.Contains(node))
                {
                    return;
                }

                // referenced means used
                fetchedVars.Remove(node.Name);
            }

            public override void Visit(CloseCursorStatement node)
            {
                string cursorName = node.Cursor.Name.Value;
                if (!cursorVars.ContainsKey(cursorName))
                {
                    return;
                }

                // was fetched but never referenced => unused
                foreach (var v in cursorVars[cursorName].Where(vv => fetchedVars.ContainsKey(vv) && !unusedVars.ContainsKey(vv)))
                {
                    unusedVars.Add(v, fetchedVars[v]);
                }
            }
        }
    }
}
