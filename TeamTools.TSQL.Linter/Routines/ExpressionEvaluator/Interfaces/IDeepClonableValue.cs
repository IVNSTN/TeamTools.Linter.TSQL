namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
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
