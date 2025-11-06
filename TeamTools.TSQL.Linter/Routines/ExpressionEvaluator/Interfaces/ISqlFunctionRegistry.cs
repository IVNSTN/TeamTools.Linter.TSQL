namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface ISqlFunctionRegistry
    {
        void RegisterFunction(SqlFunctionHandler handler);

        bool IsFunctionRegistered(string name);

        SqlFunctionHandler GetFunction(string name);
    }
}
