using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces.OperatorHandlers
{
    public interface IReverseValueSignHandler
    {
        SqlValue ReverseSign(SqlValue value);
    }
}
