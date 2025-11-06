using System;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public static class SqlFunctionHandlerFactory
    {
        public static void Initialize(ISqlFunctionRegistry funcReg)
        {
            foreach (var t in SqlFunctionClassFinder.FindClasses())
            {
                SqlFunctionHandler instance =
                    (SqlFunctionHandler)Activator.CreateInstance(t);

                funcReg.RegisterFunction(instance);
            }
        }
    }
}
