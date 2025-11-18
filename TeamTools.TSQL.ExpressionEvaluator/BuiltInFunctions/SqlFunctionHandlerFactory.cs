using System;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions
{
    public static class SqlFunctionHandlerFactory
    {
        public static void Initialize(ISqlFunctionRegistry funcReg)
        {
            foreach (var fn in SqlFunctionClassFinder.FindClasses())
            {
                funcReg.RegisterFunction((SqlFunctionHandler)Activator.CreateInstance(fn));
            }
        }
    }
}
