using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    [ExcludeFromCodeCoverage]
    public abstract class SqlConversionViolation : SqlViolation
    {
        public SqlConversionViolation(string fromType, string toType, SqlValueSource source)
        : base($"{fromType} -> {toType}", source)
        {
            FromType = fromType;
            ToType = toType;
        }

        public string FromType { get; }

        public string ToType { get; }
    }
}
