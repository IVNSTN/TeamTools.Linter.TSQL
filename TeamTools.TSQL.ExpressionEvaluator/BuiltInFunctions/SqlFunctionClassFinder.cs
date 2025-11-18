using System;
using System.Linq;
using System.Reflection;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions
{
    internal static class SqlFunctionClassFinder
    {
        public static Type[] FindClasses()
        {
            return (from t in Assembly.GetExecutingAssembly().GetTypes()
                    where !t.IsAbstract
                       && typeof(SqlFunctionHandler).IsAssignableFrom(t)
                    select t).ToArray();
        }
    }
}
