using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class MaxPrecision : LimitedIntResultFunctionHandler
    {
        private static readonly string FuncName = "@@MAX_PRECISION";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.TinyInt;
        private static readonly SqlIntValueRange MaxPrecisionRange = new SqlIntValueRange(0, 38);

        public MaxPrecision() : base(FuncName, MaxPrecisionRange, ResultTypeName)
        {
        }
    }
}
