using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces
{
    public interface ISqlFunctionRegistry
    {
        void RegisterFunction(SqlFunctionHandler handler);

        bool IsFunctionRegistered(string name);

        SqlFunctionHandler GetFunction(string name);
    }
}
