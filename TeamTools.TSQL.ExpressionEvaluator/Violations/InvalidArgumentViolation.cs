using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Properties;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class InvalidArgumentViolation : SqlViolation
    {
        public InvalidArgumentViolation(string funcName, string argName, string descr, SqlValueSource source)
        : base(string.Format(Strings.ViolationDetails_InvalidArgumentViolation_BadValue, argName, funcName, descr), source)
        {
            FuncName = funcName;
            ArgName = argName;
        }

        public string FuncName { get; }

        public string ArgName { get; }
    }
}
