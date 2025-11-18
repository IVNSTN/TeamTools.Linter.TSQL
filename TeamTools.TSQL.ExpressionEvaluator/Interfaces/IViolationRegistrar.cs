using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces
{
    public interface IViolationRegistrar
    {
        void RegisterViolation(SqlViolation violation);
    }
}
