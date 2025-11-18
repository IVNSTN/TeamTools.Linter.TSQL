using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Properties;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class RedundantFunctionArgumentViolation : SqlViolation
    {
        public RedundantFunctionArgumentViolation(string funcName, string argName, string descr, SqlValueSource source)
        : base(string.Format(Strings.ViolationDetails_RedundantFunctionArgumentViolation_OmitArg, funcName, descr), source)
        {
            FunctionName = funcName;
            ArgumentName = argName;
        }

        public string FunctionName { get; }

        public string ArgumentName { get; }
    }
}
