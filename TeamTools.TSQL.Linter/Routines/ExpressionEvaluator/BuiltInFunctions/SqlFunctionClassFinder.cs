using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    internal static class SqlFunctionClassFinder
    {
        public static IList<Type> FindClasses()
        {
            return (from t in Assembly.GetExecutingAssembly().GetTypes()
                    where typeof(SqlFunctionHandler).IsAssignableFrom(t)
                       && !t.IsAbstract
                    select t).ToList();
        }
    }
}
