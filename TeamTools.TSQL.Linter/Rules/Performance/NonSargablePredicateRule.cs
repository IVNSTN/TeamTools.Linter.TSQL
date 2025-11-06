using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0929", "NON_SARGABLE_PREDICATE")]
    internal sealed class NonSargablePredicateRule : AbstractRule, ISqlServerMetadataConsumer
    {
        private const int MaxItemsPerIssue = 5;
        private ICollection<string> builtInFunctions;

        public NonSargablePredicateRule() : base()
        {
        }

        public void LoadMetadata(SqlServerMetadata data)
        {
            builtInFunctions = new SortedSet<string>(
                data.Functions.Keys.Union(data.GlobalVariables.Keys),
                StringComparer.OrdinalIgnoreCase);
        }

        public override void Visit(WhereClause node)
        {
            ValidatePredicate(node.SearchCondition);
        }

        public override void Visit(QualifiedJoin node)
        {
            ValidatePredicate(node.SearchCondition);
        }

        private void ValidatePredicate(BooleanExpression predicate)
        {
            var nonSargablePredicates = new List<TSqlFragment>();
            ExtractNonSargablePredicates(predicate, nonSargablePredicates);

            foreach (var p in nonSargablePredicates.Take(MaxItemsPerIssue))
            {
                HandleNodeError(p);
            }
        }

        private void ExtractNonSargablePredicates(BooleanExpression node, List<TSqlFragment> predicates)
        {
            if (node is BooleanBinaryExpression bin)
            {
                ExtractNonSargablePredicates(bin.FirstExpression, predicates);
                ExtractNonSargablePredicates(bin.SecondExpression, predicates);

                return;
            }

            if (node is BooleanParenthesisExpression pexpr)
            {
                ExtractNonSargablePredicates(pexpr.Expression, predicates);

                return;
            }

            if (node is BooleanComparisonExpression cmp)
            {
                predicates.AddRange(PredicateClassifier.GetNonSargablePredicates(cmp.FirstExpression, cmp.SecondExpression, builtInFunctions));

                return;
            }

            if (node is BooleanTernaryExpression trn)
            {
                predicates.AddRange(PredicateClassifier.GetNonSargablePredicates(trn.FirstExpression, trn.SecondExpression, builtInFunctions));
                predicates.AddRange(PredicateClassifier.GetNonSargablePredicates(trn.FirstExpression, trn.ThirdExpression, builtInFunctions)
                    .Where(pred => !predicates.Contains(pred)));

                return;
            }
        }
    }
}
