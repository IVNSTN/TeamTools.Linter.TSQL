using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("SI0454", "REUSE_EXPRESSION_ALIAS")]
    internal sealed class ReuseExpressionAliasRule : AbstractRule
    {
        public ReuseExpressionAliasRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            if (node.OrderByClause is null)
            {
                return;
            }

            DetectReusableExpressions(node.SelectElements, node.OrderByClause.OrderByElements);
        }

        // TODO : should it take into account FunctionCall expressions?
        private void DetectReusableExpressions(IList<SelectElement> selectItems, IList<ExpressionWithSortOrder> orderByItems)
        {
            int n = orderByItems.Count;
            var sortExpressions = new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

            // Detecting complex expressions in ORDER BY clause
            for (int i = n - 1; i >= 0; i--)
            {
                // only somewhat complex expression are allowed
                if (BooleanExpressionPartsExtractor.ExtractExpression(orderByItems[i].Expression) is BinaryExpression expr)
                {
                    sortExpressions.Add(expr.GetFragmentCleanedText(), expr);
                }
            }

            if (sortExpressions.Count == 0)
            {
                return;
            }

            // Detecting similar expressions in SELECT list clause
            // which can be reused in ORDER BY via given alias
            for (int i = selectItems.Count - 1; i >= 0; i--)
            {
                if (selectItems[i] is SelectScalarExpression sel
                && BooleanExpressionPartsExtractor.ExtractExpression(sel.Expression) is BinaryExpression expr)
                {
                    var exprDefinition = expr.GetFragmentCleanedText();
                    if (sortExpressions.TryGetValue(exprDefinition, out var sortElement))
                    {
                        HandleNodeError(sortElement);
                    }
                }
            }
        }
    }
}
