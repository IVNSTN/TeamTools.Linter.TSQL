using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    [ExcludeFromCodeCoverage]
    public class IntentionalExceptionViolation : SqlViolation
    {
        public IntentionalExceptionViolation(string descr, SqlValueSource source)
        : base(descr, source)
        {
        }
    }
}
