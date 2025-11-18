using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    public static class ScriptDomIteratorExtension
    {
        public static IEnumerable<ScalarExpression> ExtractOutputExpressions(this SimpleCaseExpression caseExpr)
        {
            int n = caseExpr.WhenClauses.Count;
            for (int i = 0; i < n; i++)
            {
                yield return caseExpr.WhenClauses[i].ThenExpression;
            }

            if (caseExpr.ElseExpression != null)
            {
                yield return caseExpr.ElseExpression;
            }
        }

        public static IEnumerable<ScalarExpression> ExtractOutputExpressions(this SearchedCaseExpression caseExpr)
        {
            int n = caseExpr.WhenClauses.Count;
            for (int i = 0; i < n; i++)
            {
                yield return caseExpr.WhenClauses[i].ThenExpression;
            }

            if (caseExpr.ElseExpression != null)
            {
                yield return caseExpr.ElseExpression;
            }
        }

        public static IEnumerable<string> ExtractNames(this IList<ColumnReferenceExpression> columns)
        {
            int n = columns.Count;
            for (int i = 0; i < n; i++)
            {
                yield return columns[i].MultiPartIdentifier.GetLastIdentifier().Value;
            }
        }

        public static IEnumerable<string> ExtractNames(this IList<ColumnWithSortOrder> columns)
        {
            int n = columns.Count;
            for (int i = 0; i < n; i++)
            {
                yield return columns[i].Column.MultiPartIdentifier.GetLastIdentifier().Value;
            }
        }

        public static IEnumerable<string> ExtractNames(this IList<Identifier> columns)
        {
            int n = columns.Count;
            for (int i = 0; i < n; i++)
            {
                yield return columns[i].Value;
            }
        }

        private sealed class AllNodeVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment> callback;

            public AllNodeVisitor(Action<TSqlFragment> callback)
            {
                this.callback = callback;
            }

            public override void Visit(TSqlFragment node)
            {
                callback.Invoke(node);
            }
        }
    }
}
