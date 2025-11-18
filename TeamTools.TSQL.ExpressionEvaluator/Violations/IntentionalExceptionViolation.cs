using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
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
