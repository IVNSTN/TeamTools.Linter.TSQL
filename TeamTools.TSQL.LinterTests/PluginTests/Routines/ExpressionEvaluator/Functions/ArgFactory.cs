using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public static class ArgFactory
    {
        public static IList<SqlFunctionArgument> MakeList(params SqlFunctionArgument[] args)
        {
            return new List<SqlFunctionArgument>(args);
        }

        public static IList<SqlFunctionArgument> MakeListOfValues(params SqlValue[] args)
        {
            return new List<SqlFunctionArgument>(args.Select(a => new ValueArgument(a)));
        }
    }
}
