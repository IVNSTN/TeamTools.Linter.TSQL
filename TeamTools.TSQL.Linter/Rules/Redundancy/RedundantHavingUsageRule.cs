using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0193", "REDUNDANT_HAVING")]
    internal sealed class RedundantHavingUsageRule : AbstractRule
    {
        public RedundantHavingUsageRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            if (null == node.GroupByClause)
            {
                return;
            }

            if (null == node.HavingClause)
            {
                return;
            }

            var groupedCols = new List<string>();
            var havingCols = new List<string>();

            foreach (var grp in node.GroupByClause.GroupingSpecifications.OfType<ExpressionGroupingSpecification>())
            {
                if (grp.Expression is ColumnReferenceExpression colRef)
                {
                    groupedCols.Add(colRef.MultiPartIdentifier.Identifiers[colRef.MultiPartIdentifier.Identifiers.Count - 1].Value.ToUpperInvariant());
                }
            }

            var cond = new ConditionSplitter(node.HavingClause.SearchCondition);
            cond.Parse();

            foreach (var arg in cond.Arguments)
            {
                if (arg is ColumnReferenceExpression colRef && colRef.MultiPartIdentifier != null)
                {
                    havingCols.Add(colRef.MultiPartIdentifier.Identifiers[colRef.MultiPartIdentifier.Identifiers.Count - 1].Value.ToUpperInvariant());
                }
            }

            foreach (var col in havingCols)
            {
                if (groupedCols.Contains(col))
                {
                    HandleNodeError(node.HavingClause, col);
                }
            }
        }

        private class ConditionSplitter
        {
            private readonly BooleanExpression expr;
            private readonly IList<TSqlFragment> arguments = new List<TSqlFragment>();

            public ConditionSplitter(BooleanExpression expr)
            {
                this.expr = expr;
            }

            public IList<TSqlFragment> Arguments => arguments;

            public void Parse()
            {
                DoExtractExpressionFrom(expr);
            }

            private void DoRegisterArgument(ScalarExpression node)
            {
                if (node is ParenthesisExpression pe)
                {
                    DoRegisterArgument(pe.Expression);
                }
                else if (node is FunctionCall fe)
                {
                    foreach (var arg in fe.Parameters)
                    {
                        DoRegisterArgument(arg);
                    }
                }
                else if (node is IIfCall iie)
                {
                    DoRegisterArgument(iie.ThenExpression);
                    DoRegisterArgument(iie.ElseExpression);
                    DoExtractExpressionFrom(iie.Predicate);
                }
                else if (node is BinaryExpression be)
                {
                    DoRegisterArgument(be.FirstExpression);
                    DoRegisterArgument(be.SecondExpression);
                }
                else if (node is UnaryExpression ue)
                {
                    DoRegisterArgument(ue.Expression);
                }
                else
                {
                    arguments.Add(node);
                }
            }

            private void DoExtractExpressionFrom(BooleanExpression node)
            {
                if (node is BooleanBinaryExpression be)
                {
                    DoExtractExpressionFrom(be.FirstExpression);
                    DoExtractExpressionFrom(be.SecondExpression);
                }
                else
                if (node is BooleanNotExpression nee)
                {
                    DoExtractExpressionFrom(nee.Expression);
                }
                else if (node is BooleanParenthesisExpression pe)
                {
                    DoExtractExpressionFrom(pe.Expression);
                }
                else if (node is BooleanComparisonExpression ce)
                {
                    DoRegisterArgument(ce.FirstExpression);
                    DoRegisterArgument(ce.SecondExpression);
                }
                else if (node is BooleanIsNullExpression ie)
                {
                    DoRegisterArgument(ie.Expression);
                }
                else if (node is InPredicate ipre)
                {
                    DoRegisterArgument(ipre.Expression);
                }
            }
        }
    }
}
