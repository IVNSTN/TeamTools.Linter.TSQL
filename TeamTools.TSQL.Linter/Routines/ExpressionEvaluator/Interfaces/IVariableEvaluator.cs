namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface IVariableEvaluator
    {
        SqlValue GetValueAt(string varName, int tokenIndex);

        SqlTypeReference GetVariableTypeReference(string varName);
    }
}
