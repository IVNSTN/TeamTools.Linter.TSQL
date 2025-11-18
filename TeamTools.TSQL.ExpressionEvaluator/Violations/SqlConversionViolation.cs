using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public abstract class SqlConversionViolation : SqlViolation
    {
        protected SqlConversionViolation(string fromType, string toType, SqlValueSource source)
        : base($"{fromType} -> {toType}", source)
        {
            FromType = fromType;
            ToType = toType;
        }

        public string FromType { get; }

        public string ToType { get; }
    }
}
