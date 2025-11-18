using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0132", "UNION_COL_COUNT")]
    internal sealed class UnionColumnCountRule : AbstractRule
    {
        public UnionColumnCountRule() : base()
        {
        }

        public override void Visit(BinaryQueryExpression node)
        {
            int firstColCount = GetColCount(node.FirstQueryExpression);
            int secondColCount = GetColCount(node.SecondQueryExpression);

            if ((firstColCount == -1) || (secondColCount == -1))
            {
                // could not estimate column count
                return;
            }

            if (firstColCount == secondColCount)
            {
                return;
            }

            HandleNodeError(node.SecondQueryExpression);
        }

        private static int GetColCount(QueryExpression node)
        {
            while (node is BinaryQueryExpression be)
            {
                node = be.FirstQueryExpression;
            }

            if (node is QuerySpecification qs)
            {
                // TODO : if there is a simple SELECT * and FROM has only 1 subquery
                // then we can dive deeper and try to estimate nested subquery column count
                // Also it is possible to estimate SELECT * column count for
                // temp tables and table variables defined within the same batch.
                if (HasSelectStar(qs.SelectElements))
                {
                    // we don't really know how many columns are there under SELECT *
                    return -1;
                }

                return qs.SelectElements.Count;
            }

            return -1;
        }

        private static bool HasSelectStar(IList<SelectElement> sel)
        {
            for (int i = 0, n = sel.Count; i < n; i++)
            {
                if (sel[i] is SelectStarExpression)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
