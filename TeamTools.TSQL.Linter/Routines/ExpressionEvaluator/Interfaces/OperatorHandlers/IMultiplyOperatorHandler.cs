namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface IMultiplyOperatorHandler
    {
        SqlValue Multiply(SqlValue multiplicand, SqlValue multiplier);
    }
}
