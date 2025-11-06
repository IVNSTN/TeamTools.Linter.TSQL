using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0923", "SORTED_INSERT")]
    internal sealed class InsertWithOrderByRule : AbstractRule
    {
        public InsertWithOrderByRule() : base()
        {
        }

        public override void Visit(SelectInsertSource node)
        {
            ValidateQuery(node.Select);
        }

        public override void Visit(MergeSpecification node)
        {
            if (!(node.TableReference is QueryDerivedTable drv))
            {
                return;
            }

            ValidateQuery(drv.QueryExpression);
        }

        private static bool IsTop100Percent(ScalarExpression node)
        {
            if (node is ParenthesisExpression p)
            {
                return IsTop100Percent(p.Expression);
            }

            return node is IntegerLiteral top && top.Value == "100";
        }

        private void ValidateQuery(QueryExpression q)
        {
            if (!(q is QuerySpecification qs))
            {
                return;
            }

            // ORDER BY + OFFSET-FETCH is fine
            if (qs.OrderByClause is null || qs.OffsetClause != null)
            {
                return;
            }

            if (qs.TopRowFilter is null
            || (qs.TopRowFilter.Percent && IsTop100Percent(qs.TopRowFilter.Expression)))
            {
                HandleNodeError(qs.OrderByClause);
            }
        }
    }
}
