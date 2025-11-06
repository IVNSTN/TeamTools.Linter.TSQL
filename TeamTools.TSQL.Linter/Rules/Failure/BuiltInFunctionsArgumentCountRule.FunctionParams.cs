using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Functions details handler.
    /// </summary>
    internal partial class BuiltInFunctionsArgumentCountRule
    {
        private static int GetActualParamCount(PrimaryExpression node)
        {
            // some function calls below are not descendants of FunctionCall
            // thus handling them explicitly
            if (node is ParameterlessCall)
            {
                return 0;
            }

            if (node is RightFunctionCall rght)
            {
                return rght.Parameters.Count;
            }

            if (node is LeftFunctionCall lft)
            {
                return lft.Parameters.Count;
            }

            if (node is NullIfExpression)
            {
                // ScriptDom does not define "parameters" for this class
                return 2;
            }

            if (node is IIfCall)
            {
                // ScriptDome does not define "parameters" for this class
                return 3;
            }

            if (node is CoalesceExpression clsc)
            {
                return clsc.Expressions.Count;
            }

            if (node is FunctionCall fcall)
            {
                return fcall.Parameters.Count;
            }

            return -1;
        }

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
            if (!builtInFnArgCount.ContainsKey(functionName))
            {
                minParamCount = -1;
                maxParamCount = -1;
                return;
            }

            if (builtInFnArgCount[functionName].ParamCount >= 0)
            {
                minParamCount = builtInFnArgCount[functionName].ParamCount;
                maxParamCount = minParamCount;
                return;
            }

            // if passed argument count may vary
            // e.g. FORMATMESSAGE
            minParamCount = builtInFnArgCount[functionName].ParamCountMin;
            maxParamCount = builtInFnArgCount[functionName].ParamCountMax;

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
