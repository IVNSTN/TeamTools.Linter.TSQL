using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    [ExcludeFromCodeCoverage]
    public class DatePartArgument : SqlFunctionArgument
    {
        public DatePartArgument(string value)
        {
            DatePartName = value.ToUpperInvariant();
            DatePartValue = DatePartConverter.DatePartNameToEnumValue(DatePartName);
        }

        public string DatePartName { get; }

        public DatePartEnum DatePartValue { get; }
    }
}
