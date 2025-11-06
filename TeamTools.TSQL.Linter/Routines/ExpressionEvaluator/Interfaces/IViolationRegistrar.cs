namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface IViolationRegistrar
    {
        void RegisterViolation(SqlViolation violation);
    }
}
