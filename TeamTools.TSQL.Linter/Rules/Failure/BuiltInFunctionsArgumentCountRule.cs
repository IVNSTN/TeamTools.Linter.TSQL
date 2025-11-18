using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Bilt-in function argument count validation.
    /// </summary>
    // TODO : combine with ExpressionEvaluator implementations
    [RuleIdentity("FA0133", "BUILTINS_ARG_COUNT")]
    internal sealed partial class BuiltInFunctionsArgumentCountRule : AbstractRule
    {
        public BuiltInFunctionsArgumentCountRule() : base()
        {
        }

        private void Validate(PrimaryExpression node, int paramCount, string functionName = "")
        {
            Debug.Assert(builtInFnArgCount != null && builtInFnArgCount.Count > 0, "builtInFnArgCount not loaded");

            if (paramCount < 0)
            {
                // something unsupporteds
                return;
            }

            if (string.IsNullOrEmpty(functionName))
            {
                functionName = GetFunctionName(node);
            }

            ComputeParamCountRange(functionName, out int minParamCount, out int maxParamCount);
            if (minParamCount < 0)
            {
                // param count range was not provided
                return;
            }

            if ((paramCount >= minParamCount) && (paramCount <= maxParamCount))
            {
                // within range is ok
                return;
            }

            HandleNodeError(node, BuildViolationDetails(functionName, paramCount));
        }
    }
}
