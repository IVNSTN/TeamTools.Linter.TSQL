using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces.OperatorHandlers
{
    public interface IMultiplyOperatorHandler
    {
        SqlValue Multiply(SqlValue multiplicand, SqlValue multiplier);
    }
}
