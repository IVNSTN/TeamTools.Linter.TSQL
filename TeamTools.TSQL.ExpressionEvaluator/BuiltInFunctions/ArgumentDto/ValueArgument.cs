using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto
{
    [ExcludeFromCodeCoverage]
    public class ValueArgument : SqlFunctionArgument
    {
        public ValueArgument(SqlValue value)
        {
            Value = value;
        }

        public SqlValue Value { get; }
    }
}
