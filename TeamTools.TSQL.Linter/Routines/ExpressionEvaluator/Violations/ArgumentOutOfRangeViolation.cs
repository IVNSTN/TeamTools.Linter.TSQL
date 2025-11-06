using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
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
