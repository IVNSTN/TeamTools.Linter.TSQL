using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Properties;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class OutOfRangeViolation : SqlViolation
    {
        public OutOfRangeViolation(string targetType, string value, SqlValueSource source)
        : base(string.Format(MsgTemplate, value, targetType), source)
        {
            TargetType = targetType;
            Value = value;
            ValueSource = source;
        }

        public string TargetType { get; }

        public string Value { get; }

        public SqlValueSource ValueSource { get; }

        private static string MsgTemplate => Strings.ViolationDetails_OutOfRangeViolation_ValueIsOutOfTypeRange;
    }
}
