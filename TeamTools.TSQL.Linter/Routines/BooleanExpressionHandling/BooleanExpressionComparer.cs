using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines
{
    internal static class BooleanExpressionComparer
    {
        public static bool AreEqualExpressions(TSqlFragment exprA, TSqlFragment exprB)
        {
            if (exprA is null && exprB is null)
            {
                // both are nulls
                return true;
            }

            if (exprA is null || exprB is null)
            {
                // only one is null
                return false;
            }

            if (exprA is Literal literA && exprB is Literal literB)
            {
                // this is faster than building script fragment text
                return literA.Value.Equals(literB.Value, StringComparison.OrdinalIgnoreCase);
            }

            if (exprA is Literal || exprB is Literal)
            {
                // one is a literal another is not
                return false;
            }

            if (exprA is VariableReference varA && exprB is VariableReference varB)
            {
                // this is faster than building script fragment text
                return varA.Name.Equals(varB.Name, StringComparison.OrdinalIgnoreCase);
            }

            if (exprA is VariableReference || exprB is VariableReference)
            {
                // one is a variable reference another is not
                return false;
            }

            // comparing expressions lexicographically since there is no smarter option for now
            // TODO : something better and faster needed
            return string.Equals(exprA.GetFragmentCleanedText(), exprB.GetFragmentCleanedText(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool AreEqualExpressions(BooleanExpressionParts exprA, BooleanExpressionParts exprB)
        {
            return exprA.ComparisonType == exprB.ComparisonType
                && AreEqualExpressions(exprA.FirstExpression, exprB.FirstExpression)
                && AreEqualExpressions(exprA.SecondExpression, exprB.SecondExpression)
                && ((exprA.FirstExpression != null && exprA.SecondExpression != null)
                    || AreEqualExpressions(exprA.OriginalExpression, exprB.OriginalExpression));
        }
    }
}
