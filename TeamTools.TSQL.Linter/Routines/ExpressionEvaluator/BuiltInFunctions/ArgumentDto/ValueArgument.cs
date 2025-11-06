using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
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
