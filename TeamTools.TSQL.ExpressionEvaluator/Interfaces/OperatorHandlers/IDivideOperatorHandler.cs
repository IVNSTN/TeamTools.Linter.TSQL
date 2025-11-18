using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces.OperatorHandlers
{
    public interface IDivideOperatorHandler
    {
        SqlValue Divide(SqlValue dividend, SqlValue divisor);
    }
}
