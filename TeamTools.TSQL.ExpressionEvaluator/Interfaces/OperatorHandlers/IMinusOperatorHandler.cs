using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces.OperatorHandlers
{
    public interface IMinusOperatorHandler
    {
        SqlValue Subtract(SqlValue minuend, SqlValue subtrahend);
    }
}
