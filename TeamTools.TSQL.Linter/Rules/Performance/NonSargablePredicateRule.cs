using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0929", "NON_SARGABLE_PREDICATE")]
    internal sealed class NonSargablePredicateRule : AbstractRule, ISqlServerMetadataConsumer
    {
        private const int MaxItemsPerIssue = 5;
        private HashSet<string> builtInFunctions;

        public NonSargablePredicateRule() : base()
        {
        }

        public void LoadMetadata(SqlServerMetadata data)
        {
            builtInFunctions = new HashSet<string>(data.Functions.Keys, StringComparer.OrdinalIgnoreCase);
            foreach (var gv in data.GlobalVariables.Keys)
            {
                builtInFunctions.Add(gv);
            }
        }

        public override void Visit(WhereClause node) => ValidatePredicate(node.SearchCondition);

        public override void Visit(QualifiedJoin node) => ValidatePredicate(node.SearchCondition);

        private static void ExtractNonSargablePredicates(BooleanExpression node, List<TSqlFragment> predicates, HashSet<string> builtInFunctions)
        {
            if (node is BooleanBinaryExpression bin)
            {
                ExtractNonSargablePredicates(bin.FirstExpression, predicates, builtInFunctions);
                ExtractNonSargablePredicates(bin.SecondExpression, predicates, builtInFunctions);

                return;
            }

            if (node is BooleanParenthesisExpression pexpr)
            {
                ExtractNonSargablePredicates(pexpr.Expression, predicates, builtInFunctions);

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
                // note, this might produce dup items in target 'predicates' list
                predicates.AddRange(PredicateClassifier.GetNonSargablePredicates(trn.FirstExpression, trn.ThirdExpression, builtInFunctions));

                return;
            }
        }

        private void ValidatePredicate(BooleanExpression predicate)
        {
            var nonSargablePredicates = new List<TSqlFragment>();
            ExtractNonSargablePredicates(predicate, nonSargablePredicates, builtInFunctions);

            if (nonSargablePredicates.Count == 0)
            {
                return;
            }

            ReportViolations(nonSargablePredicates);
        }

        private void ReportViolations(List<TSqlFragment> nonSargablePredicates)
        {
            int n = nonSargablePredicates.Count;
            int violations = 0;
            for (int i = 0; i < n && violations < MaxItemsPerIssue; i++)
            {
                HandleNodeError(nonSargablePredicates[i]);
                violations++;
            }
        }
    }
}
