using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Functions details handler.
    /// </summary>
    internal partial class BuiltInFunctionsArgumentCountRule
    {
        public override void Visit(ParameterlessCall node) => Validate(node, 0);

        public override void Visit(FunctionCall node) => Validate(node, node.Parameters.Count);

        public override void Visit(RightFunctionCall node) => Validate(node, node.Parameters.Count, "RIGHT");

        public override void Visit(LeftFunctionCall node) => Validate(node, node.Parameters.Count, "LEFT");

        public override void Visit(CoalesceExpression node) => Validate(node, node.Expressions.Count, "COALESCE");

        public override void Visit(NullIfExpression node) => Validate(node, 2, "NULLIF");

        public override void Visit(IIfCall node) => Validate(node, 3, "IIF");

        private static string GetFunctionName(TSqlFragment node)
        {
            if (node is FunctionCall fn)
            {
                if (fn.CallTarget != null && !(fn.CallTarget is MultiPartIdentifierCallTarget mid
                && mid.MultiPartIdentifier.Identifiers.Count == 1))
                {
                    // SqlXml functions e.g. <xml col>.value
                    // <xml col> here is CallTarget. Regular function do not have call target
                    return default;
                }

                return fn.FunctionName.Value;
            }

            if (node.FirstTokenIndex < 0)
            {
                // some parser bug
                return default;
            }

            var token = node.ScriptTokenStream[node.FirstTokenIndex];
            if (!TokenTypes.Contains(token.TokenType))
            {
                return default;
            }

            return token.Text;
        }

        private void ComputeParamCountRange(string functionName, out int minParamCount, out int maxParamCount)
        {
            if (string.IsNullOrEmpty(functionName)
            || !builtInFnArgCount.TryGetValue(functionName, out var funcSignature))
            {
                minParamCount = -1;
                maxParamCount = -1;
                return;
            }

            if (funcSignature.ParamCount >= 0)
            {
                minParamCount = funcSignature.ParamCount;
                maxParamCount = minParamCount;
                return;
            }

            // if passed argument count may vary
            // e.g. FORMATMESSAGE
            minParamCount = funcSignature.ParamCountMin;
            maxParamCount = funcSignature.ParamCountMax;

            // if both are negative then no info provided
            if (maxParamCount < 0 && minParamCount >= 0)
            {
                maxParamCount = int.MaxValue;
            }
            else if (minParamCount < 0 && maxParamCount >= 0)
            {
                minParamCount = 0;
            }
        }
    }
}
