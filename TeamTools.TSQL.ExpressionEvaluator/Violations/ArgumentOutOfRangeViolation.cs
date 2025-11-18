using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class ArgumentOutOfRangeViolation : InvalidArgumentViolation
    {
        public ArgumentOutOfRangeViolation(string funcName, string argName, string descr, SqlValueSource source)
        : base(funcName, argName, descr, source)
        {
        }
    }
}
