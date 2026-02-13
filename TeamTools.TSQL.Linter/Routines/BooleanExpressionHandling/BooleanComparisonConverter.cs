using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines
{
    [ExcludeFromCodeCoverage]
    internal static class BooleanComparisonConverter
    {
        public static BooleanComparisonType RevertComparison(BooleanComparisonType cmp)
        {
            switch (cmp)
            {
                case BooleanComparisonType.Equals:
                    // Aligning not equality checks to "<>" format
                    return BooleanComparisonType.NotEqualToBrackets;

                case BooleanComparisonType.NotEqualToBrackets:
                case BooleanComparisonType.NotEqualToExclamation:
                    return BooleanComparisonType.Equals;

                case BooleanComparisonType.GreaterThan:
                    return BooleanComparisonType.LessThanOrEqualTo;

                case BooleanComparisonType.GreaterThanOrEqualTo:
                    return BooleanComparisonType.LessThan;

                case BooleanComparisonType.LessThanOrEqualTo:
                    return BooleanComparisonType.GreaterThan;

                case BooleanComparisonType.LessThan:
                    return BooleanComparisonType.GreaterThanOrEqualTo;

                case BooleanComparisonType.NotGreaterThan:
                    return BooleanComparisonType.GreaterThan;

                case BooleanComparisonType.NotLessThan:
                    return BooleanComparisonType.LessThan;

                // TODO : or fail?
                default:
                    return cmp;
            }
        }

        public static string ComparisonToString(BooleanComparisonType cmp)
        {
            switch (cmp)
            {
                case BooleanComparisonType.Equals:
                    return "=";

                case BooleanComparisonType.NotEqualToBrackets:
                    return "<>";

                case BooleanComparisonType.NotEqualToExclamation:
                    return "!=";

                case BooleanComparisonType.GreaterThan:
                    return ">";

                case BooleanComparisonType.GreaterThanOrEqualTo:
                    return ">=";

                case BooleanComparisonType.LessThanOrEqualTo:
                    return "<=";

                case BooleanComparisonType.LessThan:
                    return "<";

                case BooleanComparisonType.NotGreaterThan:
                    return "!>";

                case BooleanComparisonType.NotLessThan:
                    return "!<";

                // TODO : or fail?
                default:
                    return cmp.ToString();
            }
        }

        public static BooleanComparisonType ToEqualityComparison(bool isEqual)
        {
            return isEqual ? BooleanComparisonType.Equals : BooleanComparisonType.NotEqualToBrackets;
        }
    }
}
