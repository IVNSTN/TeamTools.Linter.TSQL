using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0850", "EXTRA_WHERE_PREDICATE")]
    internal class WherePredicateSameAsJoinPredicateRule : AbstractRule
    {
        public WherePredicateSameAsJoinPredicateRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            if (node.WhereClause?.SearchCondition is null)
            {
                // no WHERE or it is a WHERE CURRENT OF <cursor>
                return;
            }

            if ((node.FromClause?.TableReferences?.Count ?? 0) == 0)
            {
                // no FROM
                return;
            }

            var innerJoinPredicates = ExtractInnerJoinPredicates(node.FromClause.TableReferences);

            if (innerJoinPredicates.Count == 0)
            {
                // no outer joins
                return;
            }

            DetectDupPredicates(node.WhereClause.SearchCondition, innerJoinPredicates);
        }

        // Extracting all predicates from INNER JOINS defined in a query
        private static ICollection<BooleanExpressionParts> ExtractInnerJoinPredicates(IList<TableReference> joins)
        {
            var joinPredicates = new List<BooleanExpressionParts>();

            for (int i = joins.Count - 1; i >= 0; i--)
            {
                var join = joins[i];
                if (join is JoinTableReference jtr)
                {
                    foreach (var predicate in ExtractInnerJoinPredicates(jtr))
                    {
                        joinPredicates.Add(predicate);
                    }
                }
            }

            return joinPredicates;
        }

        // Extracting join predicates recursively
        private static IEnumerable<BooleanExpressionParts> ExtractInnerJoinPredicates(JoinTableReference join)
        {
            if (join is QualifiedJoin qj && qj.QualifiedJoinType == QualifiedJoinType.Inner)
            {
                foreach (var predicate in ExtractNestedPredicates(qj.SearchCondition))
                {
                    yield return predicate;
                }
            }

            // Expanding linked joins recursively
            if (join.FirstTableReference is JoinTableReference fjt)
            {
                foreach (var predicate in ExtractInnerJoinPredicates(fjt))
                {
                    yield return predicate;
                }
            }

            if (join.SecondTableReference is JoinTableReference sjt)
            {
                foreach (var predicate in ExtractInnerJoinPredicates(sjt))
                {
                    yield return predicate;
                }
            }
        }

        // Splitting complex predicate into atomic boolean expressions with normalization
        private static IEnumerable<BooleanExpressionParts> ExtractNestedPredicates(BooleanExpression expr)
        {
            while (expr is BooleanParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            // AND means that all predicates are supposed to be applied. OR is not supported.
            if (expr is BooleanBinaryExpression bin && bin.BinaryExpressionType == BooleanBinaryExpressionType.And)
            {
                foreach (var predicate in ExtractNestedPredicates(bin.FirstExpression))
                {
                    yield return predicate;
                }

                foreach (var predicate in ExtractNestedPredicates(bin.SecondExpression))
                {
                    yield return predicate;
                }
            }
            else
            {
                var predicate = BooleanExpressionNormalizer.Normalize(expr);

                if (predicate != null && predicate.FirstExpression != null && predicate.SecondExpression != null)
                {
                    yield return predicate;
                }
            }
        }

        // Splitting WHERE predicates into atomic expressions and comparing them to previously extracted
        // predicates from INNER JOINs
        private void DetectDupPredicates(BooleanExpression wherePredicate, ICollection<BooleanExpressionParts> joinPredicates)
        {
            foreach (var where in ExtractNestedPredicates(wherePredicate))
            {
                if (joinPredicates.Contains(where))
                {
                    HandleNodeError(where.FirstExpression);
                }
            }
        }
    }
}
