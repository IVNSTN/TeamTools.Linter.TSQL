using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces
{
    public interface IDeepClonableValue<TValue>
    where TValue : SqlValue
    {
        TValue DeepClone();
    }

    public interface IClonableValue
    {
        SqlValue Clone();
    }
}
