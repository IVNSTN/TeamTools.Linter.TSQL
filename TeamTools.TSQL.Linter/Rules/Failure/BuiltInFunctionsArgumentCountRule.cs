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

        public override void Visit(PrimaryExpression node)
        {
            Debug.Assert(builtInFnArgCount != null, "builtInFnArgCount not set");
            if (builtInFnArgCount == null)
            {
                return;
            }

            int paramCount = GetActualParamCount(node);
            if (paramCount < 0)
            {
                // could not determine
                return;
            }

            string functionName = GetFunctionName(node);
            if (string.IsNullOrEmpty(functionName) || !builtInFnArgCount.ContainsKey(functionName))
            {
                // unknown function
                return;
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
