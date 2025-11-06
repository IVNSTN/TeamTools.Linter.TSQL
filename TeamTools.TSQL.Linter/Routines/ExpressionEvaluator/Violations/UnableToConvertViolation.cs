using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    [ExcludeFromCodeCoverage]
    public class UnableToConvertViolation : SqlConversionViolation
    {
        public UnableToConvertViolation(string value, string targetType, SqlValueSource source)
        : base(value, targetType, source)
        {
        }
    }
}
