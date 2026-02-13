using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines
{
    // TODO : Support: BETWEEN, [NOT] IN, [NOT] LIKE, IS [NOT] NULL
    internal sealed class BooleanExpressionParts : IEquatable<BooleanExpressionParts>
    {
        public ScalarExpression FirstExpression { get; set; }

        public ScalarExpression SecondExpression { get; set; }

        public BooleanExpression OriginalExpression { get; set; }

        public BooleanComparisonType ComparisonType { get; set; } = BooleanComparisonType.Equals;

        public override string ToString()
        {
            return string.Format(
                "{0} {1} {2}",
                FirstExpression.GetFragmentCleanedText(),
                ComparisonToString(ComparisonType, SecondExpression is null),
                SecondExpression?.GetFragmentCleanedText() ?? "");
        }

        public override bool Equals(object other) => Equals(other as BooleanExpressionParts);

        public bool Equals(BooleanExpressionParts other)
        {
            if (other is null)
            {
                return false;
            }

            if (ComparisonType != other.ComparisonType)
            {
                return false;
            }

            if ((SecondExpression is null) != (other.SecondExpression is null))
            {
                return false;
            }

            string firstName = ExtractExpressionName(FirstExpression);
            string otherFirstName = ExtractExpressionName(other.FirstExpression);

            if (string.IsNullOrEmpty(firstName) != string.IsNullOrEmpty(otherFirstName))
            {
                return false;
            }

            string secondName = ExtractExpressionName(SecondExpression);
            string otherSecondName = ExtractExpressionName(other.SecondExpression);

            if (string.IsNullOrEmpty(secondName) != string.IsNullOrEmpty(otherSecondName))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(firstName) && (SecondExpression is null || string.IsNullOrEmpty(secondName)))
            {
                return string.Equals(firstName, otherFirstName, StringComparison.OrdinalIgnoreCase)
                    && (SecondExpression is null || string.Equals(secondName, otherSecondName, StringComparison.OrdinalIgnoreCase));
            }

            if (string.IsNullOrEmpty(firstName))
            {
                firstName = FirstExpression.GetFragmentCleanedText();
            }

            if (string.IsNullOrEmpty(otherFirstName))
            {
                otherFirstName = other.FirstExpression.GetFragmentCleanedText();
            }

            if (SecondExpression != null)
            {
                if (string.IsNullOrEmpty(secondName))
                {
                    secondName = SecondExpression.GetFragmentCleanedText();
                }

                if (string.IsNullOrEmpty(otherSecondName))
                {
                    otherSecondName = other.SecondExpression.GetFragmentCleanedText();
                }
            }

            return string.Equals(firstName, otherFirstName, StringComparison.OrdinalIgnoreCase)
                && (SecondExpression is null || string.Equals(secondName, otherSecondName, StringComparison.OrdinalIgnoreCase));
        }

        public BooleanExpressionParts Mirror()
        {
            if (SecondExpression is null)
            {
                // NOT NULL expression cannot be mirrored
                return this;
            }

            return new BooleanExpressionParts
            {
                FirstExpression = SecondExpression,
                SecondExpression = FirstExpression,
                OriginalExpression = OriginalExpression,
                ComparisonType = BooleanComparisonConverter.RevertComparison(ComparisonType),
            };
        }

        private static string ComparisonToString(BooleanComparisonType comparison, bool isComparisonToNull)
        {
            if (isComparisonToNull)
            {
                return comparison == BooleanComparisonType.Equals ? "IS NULL" : "IS NOT NULL";
            }

            return BooleanComparisonConverter.ComparisonToString(comparison);
        }

        private static string ExtractExpressionName(ScalarExpression node)
        {
            if (node is VariableReference varRef)
            {
                return varRef.Name;
            }
            else if (node is ColumnReferenceExpression colRef)
            {
                return colRef?.MultiPartIdentifier.Identifiers.GetFullName();
            }
            else if (node is StringLiteral str)
            {
                // String literals are stored in ScriptDom without quotes
                // escaping is not related here since we're not going to generate legal code from it
                return string.Format("'{0}'", str.Value);
            }
            else if (node is Literal literal)
            {
                return literal.Value;
            }

            return default;
        }
    }
}
