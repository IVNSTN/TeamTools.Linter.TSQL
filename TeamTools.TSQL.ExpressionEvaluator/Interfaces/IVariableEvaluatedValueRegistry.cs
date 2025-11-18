using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces
{
    public interface IVariableEvaluatedValueRegistry
    {
        void RegisterEvaluatedValue(string varName, int tokenIndex, SqlValue value);

        void RegisterEvaluatedValue(string varName, int tokenIndex, SqlValueKind value, SqlValueSource src);

        void ResetEvaluatedValuesAfterBlock(int fromTokenIndex, int tillTokenIndex, SqlValueSource src);

        void RevertValueEstimatesToBeforeBlock(int fromTokenIndex, int tillTokenIndex);

        void Squash();
    }
}
