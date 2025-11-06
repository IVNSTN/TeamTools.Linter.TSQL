namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface IDivideOperatorHandler
    {
        SqlValue Divide(SqlValue dividend, SqlValue divisor);
    }
}
