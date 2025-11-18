using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces
{
    public interface IVariableEvaluator
    {
        SqlValue GetValueAt(string varName, int tokenIndex);

        SqlTypeReference GetVariableTypeReference(string varName);
    }
}
