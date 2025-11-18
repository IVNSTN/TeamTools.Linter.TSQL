using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public static class ArgFactory
    {
        public static List<SqlFunctionArgument> MakeList(params SqlFunctionArgument[] args)
        {
            return new List<SqlFunctionArgument>(args);
        }

        public static List<SqlFunctionArgument> MakeListOfValues(params SqlValue[] args)
        {
            return new List<SqlFunctionArgument>(args.Select(a => new ValueArgument(a)));
        }
    }
}
