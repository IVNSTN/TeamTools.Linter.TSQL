using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    [ExcludeFromCodeCoverage]
    public class InvalidArgumentViolation : SqlViolation
    {
        public InvalidArgumentViolation(string funcName, string argName, string descr, SqlValueSource source)
        : base($"Argument '{argName}' has invalid value for {funcName.ToUpperInvariant()} {descr}", source)
        {
            FuncName = funcName;
            ArgName = argName;
        }

        public string FuncName { get; }

        public string ArgName { get; }
    }
}
