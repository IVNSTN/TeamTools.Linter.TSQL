namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface IPlusOperatorHandler
    {
        SqlValue Sum(SqlValue augend, SqlValue addend);
    }
}
