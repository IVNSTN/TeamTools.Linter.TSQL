using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    [ExcludeFromCodeCoverage]
    public class RedundantTypeConversionViolation : SqlViolation
    {
        public RedundantTypeConversionViolation(string message, SqlValueSource source)
        : base(message, source)
        {
        }
    }
}
