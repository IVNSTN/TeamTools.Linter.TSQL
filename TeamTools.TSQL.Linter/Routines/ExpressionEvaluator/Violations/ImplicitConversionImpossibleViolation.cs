using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    [ExcludeFromCodeCoverage]
    public class ImplicitConversionImpossibleViolation : SqlConversionViolation
    {
        public ImplicitConversionImpossibleViolation(string fromType, string toType, SqlValueSource source)
        : base(fromType, toType, source)
        {
        }
    }
}
