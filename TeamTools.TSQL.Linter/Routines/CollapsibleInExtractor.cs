using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines
{
    /// <summary>
    /// It detects IN/NOT IN predicates that can be combined into single one
    /// 1) foo IN (1, 2, 3) OR foo IN (4, 5) => foo IN (1, 2, 3, 4, 5)
    /// 2) foo NOT IN (1, 2, 3) AND foo NOT IN (4, 5) => foo NOT IN (1, 2, 3, 4, 5)
    /// </summary>
    internal class CollapsibleInExtractor
    {
        private readonly bool notPresence;
        private readonly BooleanBinaryExpressionType binaryOperator;

        public CollapsibleInExtractor(bool notPresence)
        {
            this.notPresence = notPresence;
            binaryOperator = notPresence ? BooleanBinaryExpressionType.And : BooleanBinaryExpressionType.Or;
        }

        public void Process(BooleanBinaryExpression node, Action<TSqlFragment, string> callback)
        {
            if (node.BinaryExpressionType != binaryOperator)
            {
                return;
            }

            var predicates = ExpandPredicates(node.FirstExpression)
                .Union(ExpandPredicates(node.SecondExpression))
                .ToList();

            var alreadyMentionedLeftSide = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = predicates.Count - 1; i >= 0; i--)
            {
                var predicate = predicates[i];

                if (!alreadyMentionedLeftSide.Add(predicate.FilteredItemName))
                {
                    callback(predicate.Node, predicate.FilteredItemName);
                }
            }
        }

        private IEnumerable<InPredicateInfo> ExpandPredicates(BooleanExpression node)
        {
            if (node is BooleanBinaryExpression bin && bin.BinaryExpressionType == binaryOperator)
            {
                return ExpandPredicates(bin.FirstExpression).
                    Union(ExpandPredicates(bin.SecondExpression));
            }

            return ExpandExpression(node);
        }

        private IEnumerable<InPredicateInfo> ExpandExpression(BooleanExpression node)
        {
            while (node is BooleanParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (!(node is InPredicate inPredicate && inPredicate.Values != null))
            {
                // ... IN (subquery) cannot be easily combined
                // only IN (values,..) supported
                yield break;
            }

            if (inPredicate.NotDefined != notPresence)
            {
                yield break;
            }

            var leftSide = inPredicate.Expression.ExtractScalarExpression();
            string filteredItemName;

            // On the left side there must be a variable or a column reference
            if (leftSide is VariableReference filteredVarRef)
            {
                filteredItemName = filteredVarRef.Name;
            }
            else if (leftSide is ColumnReferenceExpression filteredColRef
            && filteredColRef.MultiPartIdentifier != null)
            {
                // filteredColRef.MultiPartIdentifier can be null for sys columns like $action
                filteredItemName = filteredColRef.MultiPartIdentifier.Identifiers.GetFullName();
            }
            else
            {
                yield break;
            }

            yield return new InPredicateInfo
            {
                Node = leftSide,
                FilteredItemName = filteredItemName,
            };
        }

        private sealed class InPredicateInfo
        {
            public TSqlFragment Node { get; set; }

            public string FilteredItemName { get; set; }
        }
    }
}
