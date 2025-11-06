namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface IMinusOperatorHandler
    {
        SqlValue Subtract(SqlValue minuend, SqlValue subtrahend);
    }
}
