using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto
{
    [ExcludeFromCodeCoverage]
    public class DatePartArgument : SqlFunctionArgument
    {
        public DatePartArgument(string value)
        {
            DatePartName = DatePartConverter.SupportedDateParts[value];
            DatePartValue = DatePartConverter.DatePartNameToEnumValue(DatePartName);
        }

        public string DatePartName { get; }

        public DatePartEnum DatePartValue { get; }
    }
}
