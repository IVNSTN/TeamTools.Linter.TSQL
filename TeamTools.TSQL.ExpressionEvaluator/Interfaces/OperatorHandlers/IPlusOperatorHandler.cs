using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces.OperatorHandlers
{
    public interface IPlusOperatorHandler
    {
        SqlValue Sum(SqlValue augend, SqlValue addend);
    }
}
