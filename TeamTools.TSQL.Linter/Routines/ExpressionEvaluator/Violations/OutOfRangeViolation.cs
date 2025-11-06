using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    [ExcludeFromCodeCoverage]
    public class OutOfRangeViolation : SqlViolation
    {
        public OutOfRangeViolation(string targetType, string value, SqlValueSource source)
        : base($"{value} is out of range for {targetType}", source)
        {
            TargetType = targetType;
            Value = value;
            ValueSource = source;
        }

        public string TargetType { get; }

        public string Value { get; }

        public SqlValueSource ValueSource { get; }
    }
}
