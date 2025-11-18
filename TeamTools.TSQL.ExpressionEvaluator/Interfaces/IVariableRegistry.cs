using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces
{
    public interface IVariableRegistry
    {
        void RegisterVariable(string varName, SqlTypeReference varType);

        bool IsVariableRegistered(string varName);
    }
}
