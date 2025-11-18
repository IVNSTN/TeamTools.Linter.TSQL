using System.Numerics;

namespace TeamTools.TSQL.ExpressionEvaluator.Values
{
    public static class SqlValueCompareResultConverter
    {
        public static SqlValueCompareResult ToCompareResult(this int comparison)
        {
            if (comparison > 0)
            {
                return SqlValueCompareResult.Greater;
            }
            else if (comparison < 0)
            {
                return SqlValueCompareResult.Less;
            }
            else
            {
                return SqlValueCompareResult.Equal;
            }
        }

        public static SqlValueCompareResult ToCompareResult(this BigInteger comparison)
        {
            if (comparison > 0)
            {
                return SqlValueCompareResult.Greater;
            }
            else if (comparison < 0)
            {
                return SqlValueCompareResult.Less;
            }
            else
            {
                return SqlValueCompareResult.Equal;
            }
        }
    }
}
