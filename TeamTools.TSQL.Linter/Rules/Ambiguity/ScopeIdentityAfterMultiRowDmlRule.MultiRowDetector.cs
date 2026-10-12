using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Rules
{
    internal sealed partial class ScopeIdentityAfterMultiRowDmlRule
    {
        private static class MultiRowDmlDetector
        {
            public static bool IsMultiRowDml(InsertSpecification node)
            {
                return !HasTopLimit(node) && HasMultiRowSource(node);
            }

            public static bool IsMultiRowDml(MergeSpecification node)
            {
                return !HasTopLimit(node) && HasMultiRowSource(node);
            }

            private static bool HasTopLimit(DataModificationSpecification spec)
            {
                if (spec.OutputIntoClause != null)
                {
                    // Valid ID storage approach applied thus ignoring SCOPE_IDENTITY
                    // called afterwards for whatever reason
                    return true;
                }

                // In case if there is TOP (1) DML (not source query) modifier - not multiline.
                // Otherwise need to look at DML source query.
                return IsTopOne(spec.TopRowFilter);
            }

            private static bool IsTopOne(TopRowFilter top)
            {
                if (top is null)
                {
                    return false;
                }

                var topExpression = top.Expression;
                while (topExpression is ParenthesisExpression pe)
                {
                    topExpression = pe.Expression;
                }

                if (topExpression is Literal topLiteral
                && int.TryParse(topLiteral.Value, out int topLimit))
                {
                    // TOP 0 is not multiline for sure
                    return topLimit == 1 || topLimit == 0;
                }

                return false;
            }

            private static bool IsMultiRowQuery(QueryExpression query)
            {
                if (!(query is QuerySpecification spec))
                {
                    // binary query expression is probably multiline (UNION, EXCEPT)
                    return true;
                }

                if (spec.FromClause is null)
                {
                    // Scalar SELECT with no FROM
                    return false;
                }

                if (IsTopOne(spec.TopRowFilter))
                {
                    // SELECT TOP 1
                    return false;
                }

                if (spec.FromClause.TableReferences.Count > 1)
                {
                    // Many sources with joins - probably multiline
                    return true;
                }

                var firstSource = spec.FromClause.TableReferences[0];

                if (firstSource is InlineDerivedTable inline)
                {
                    // FROM VALUES + 1 row - no multiline
                    return inline.RowValues.Count > 1;
                }

                if (firstSource is QueryDerivedTable subquery)
                {
                    // going deeper
                    return IsMultiRowQuery(subquery.QueryExpression);
                }

                // no limit - multiline
                return true;
            }

            private static bool HasMultiRowSource(InsertSpecification spec)
            {
                if (spec.InsertSource is ValuesInsertSource values)
                {
                    // Single value insert
                    return values.RowValues.Count > 1;
                }

                if (spec.InsertSource is ExecuteInsertSource)
                {
                    // INSERT-EXEC is probably a multiline operation
                    return true;
                }

                if (spec.InsertSource is SelectInsertSource insSrc)
                {
                    return IsMultiRowQuery(insSrc.Select);
                }

                // unsupported case - avoid false-positive violations
                return false;
            }

            private static bool HasMultiRowSource(MergeSpecification spec)
            {
                if (!(spec.TableReference is QueryDerivedTable q))
                {
                    // Merge source is not a subquery thus a direct table reference
                    // with unknown number of rows
                    return true;
                }

                return IsMultiRowQuery(q.QueryExpression);
            }
        }
    }
}
