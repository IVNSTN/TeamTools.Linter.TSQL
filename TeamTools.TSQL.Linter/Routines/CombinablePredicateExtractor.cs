using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines
{
    /// <summary>
    /// It detects predicates that can be combined into single IN/NOT IN expression
    /// 1) A != B AND A != C   => A NOT IN (B, C)
    /// 2) A = B OR A = C      => A IN (B, C)
    /// </summary>
    internal class CombinablePredicateExtractor
    {
        private static readonly string ExampleTemplateWithAllValues = "{0} IN ({1})";
        private static readonly string ExampleTemplateWithSomeValues = "{0} IN ({1},..)";
        private static readonly string ExampleTemplateWithAllValuesNegated = "{0} NOT IN ({1})";
        private static readonly string ExampleTemplateWithSomeValuesNegated = "{0} NOT IN ({1},..)";

        private static readonly int MinValuesToCollapse = 3;
        private static readonly int MaxValuesToShow = 5;

        private readonly bool notPresence;
        private readonly BooleanBinaryExpressionType binaryOperator;
        private readonly BooleanComparisonType equalityOperator;
        private readonly Func<bool, string> getMsgTemplate;

        public CombinablePredicateExtractor(bool notPresence)
        {
            if (notPresence)
            {
                this.notPresence = true;
                binaryOperator = BooleanBinaryExpressionType.And;
                // Note, there is also NotEqualToExclamation
                // and a separate rule which forces to use <> instead of !=
                equalityOperator = BooleanComparisonType.NotEqualToBrackets;
                getMsgTemplate = GetMsgTemplateNegated;
            }
            else
            {
                this.notPresence = false;
                binaryOperator = BooleanBinaryExpressionType.Or;
                equalityOperator = BooleanComparisonType.Equals;
                getMsgTemplate = GetMsgTemplate;
            }
        }

        public void Process(BooleanBinaryExpression node, Action<TSqlFragment, string> callback)
        {
            if (node.BinaryExpressionType != binaryOperator)
            {
                return;
            }

            // TODO : simplification and optimization required
            // Extracting nested simple predicates:
            //   - "A = B" combined with 'OR' operator
            //   - "A != B" combined with 'AND' operator
            // and composing full list of options for filtered elements.
            // In the output dictionary the Key will be the filtered element name
            // and the Value - list of options which can be put into (NOT) IN predicate values.
            var equalityComparisons = ExpandPredicates(node.FirstExpression)
                .Union(ExpandPredicates(node.SecondExpression))
                .GroupBy(cmp => cmp.FilteredItem, StringComparer.OrdinalIgnoreCase)
                .Where(grp => grp.Count() >= MinValuesToCollapse && grp.Max(v => v.ShouldBeReported))
                .ToDictionary(
                    grp => grp.Key,
                    grp => grp
                        .Where(s => !string.IsNullOrEmpty(s.FilterValueText))
                        .DistinctBy(value => value.FilterValueText, StringComparer.OrdinalIgnoreCase)
                        .OrderBy(value => value.FilterValueText, StringComparer.OrdinalIgnoreCase)
                        .ToList(),
                    StringComparer.OrdinalIgnoreCase);

            // Composing messages with recommended IN/NOT IN expression
            foreach (var collapse in equalityComparisons)
            {
                string values = string.Join(
                    ", ",
                    collapse.Value
                        .Select(v => v.FilterValueText)
                        .Take(MaxValuesToShow));

                string exampleTemplate = getMsgTemplate(collapse.Value.Count > MaxValuesToShow);

                callback.Invoke(
                    collapse.Value[0].FilterValue,
                    string.Format(exampleTemplate, collapse.Key, values));
            }
        }

        private static string GetMsgTemplate(bool limitedNumberOfelements)
        {
            return limitedNumberOfelements
                ? ExampleTemplateWithSomeValues
                : ExampleTemplateWithAllValues;
        }

        private static string GetMsgTemplateNegated(bool limitedNumberOfelements)
        {
            return limitedNumberOfelements
                ? ExampleTemplateWithSomeValuesNegated
                : ExampleTemplateWithAllValuesNegated;
        }

        /// <summary>
        /// It will return only equality comparisons combined with OR operator.
        /// On one of the sides there must be a column reference or variable
        /// and on the other side either literal or variable.
        /// </summary>
        private IEnumerable<CombinablePredicate> ExpandPredicates(BooleanExpression node)
        {
            if (node is BooleanBinaryExpression bin)
            {
                if (bin.BinaryExpressionType != binaryOperator)
                {
                    return Enumerable.Empty<CombinablePredicate>();
                }

                return ExpandPredicates(bin.FirstExpression).
                    Union(ExpandPredicates(bin.SecondExpression));
            }

            return ExpandExpression(node);
        }

        private IEnumerable<CombinablePredicate> ExpandExpression(BooleanExpression node)
        {
            while (node is BooleanParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is BooleanComparisonExpression cmp && cmp.ComparisonType == equalityOperator)
            {
                var leftSide = cmp.FirstExpression.ExtractScalarExpression();
                var rightSide = cmp.SecondExpression.ExtractScalarExpression();

                var predicate = CombinablePredicate.Make(leftSide, rightSide);
                if (predicate != null)
                {
                    yield return predicate;
                }

                // Trying both directions of comparison
                predicate = CombinablePredicate.Make(rightSide, leftSide);
                if (predicate != null)
                {
                    yield return predicate;
                }
            }
            else if (node is InPredicate inPredicate && inPredicate.Values != null
            && inPredicate.NotDefined == notPresence)
            {
                var leftSide = inPredicate.Expression.ExtractScalarExpression();

                // TODO : not sure if exposing IN back to separate predicates is a good idea
                for (int i = inPredicate.Values.Count - 1; i >= 0; i--)
                {
                    var rightSide = inPredicate.Values[i].ExtractScalarExpression();

                    var predicate = CombinablePredicate.Make(leftSide, rightSide);
                    if (predicate != null)
                    {
                        // IN values are no evil
                        predicate.ShouldBeReported = false;
                        yield return predicate;
                    }
                }
            }
        }

        /// <summary>
        /// It represents (filtered item, filter value) pair.
        /// </summary>
        private sealed class CombinablePredicate
        {
            public CombinablePredicate(string filteredItem, VariableReference filterValue) : this(filteredItem, (TSqlFragment)filterValue)
            {
                FilterValueText = filterValue.Name;
            }

            public CombinablePredicate(string filteredItem, Literal filterValue) : this(filteredItem, (TSqlFragment)filterValue)
            {
                if (filterValue is StringLiteral)
                {
                    // String literals are stored in ScriptDom deserialized, without quotation.
                    // To print it with an IN predicate example quotation is needed back. Nested quotes should be escaped.
                    FilterValueText = "'" + filterValue.Value.Replace("'", "''") + "'";
                }
                else
                {
                    FilterValueText = filterValue.Value;
                }
            }

            public CombinablePredicate(string filteredItem, string filterValue, TSqlFragment filterNode) : this(filteredItem, filterNode)
            {
                FilterValueText = filterValue;
            }

            private CombinablePredicate(string filteredItem, TSqlFragment filterValue)
            {
                FilteredItem = filteredItem ?? throw new ArgumentNullException(nameof(filteredItem));
                FilterValue = filterValue ?? throw new ArgumentNullException(nameof(filterValue));
            }

            public string FilteredItem { get; }

            public TSqlFragment FilterValue { get; }

            public string FilterValueText { get; }

            public bool ShouldBeReported { get; set; } = true;

            public static CombinablePredicate Make(ScalarExpression filteredItem, ScalarExpression filterValue)
            {
                string filteredItemName;

                // On the left side there must be a variable or a column reference
                if (filteredItem is VariableReference filteredVarRef)
                {
                    filteredItemName = filteredVarRef.Name;
                }
                else if (filteredItem is ColumnReferenceExpression filteredColRef
                && filteredColRef.MultiPartIdentifier != null)
                {
                    // filteredColRef.MultiPartIdentifier can be null for sys columns like $action
                    filteredItemName = filteredColRef.MultiPartIdentifier.Identifiers.GetFullName();
                }
                else
                {
                    return default;
                }

                // On the right side there must be something simple: literal or variable
                if (filterValue is VariableReference filterValueAsVar)
                {
                    return new CombinablePredicate(filteredItemName, filterValueAsVar);
                }
                else if (filterValue is Literal filterValueAsLiteral)
                {
                    return new CombinablePredicate(filteredItemName, filterValueAsLiteral);
                }

                // Any other case is not supported. Complex IN predicate values are not welcome.
                return default;
            }
        }
    }
}
