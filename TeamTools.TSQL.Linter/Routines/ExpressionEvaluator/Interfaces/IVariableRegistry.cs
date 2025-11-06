namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface IVariableRegistry
    {
        void RegisterVariable(string varName, SqlTypeReference varType);

        bool IsVariableRegistered(string varName);
    }
}
