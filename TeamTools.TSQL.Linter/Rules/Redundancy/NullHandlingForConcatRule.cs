using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0721", "NULL_HANDLING_FOR_CONCAT")]
    internal sealed class NullHandlingForConcatRule : AbstractRule
    {
        private static readonly HashSet<string> ConcatFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CONCAT",
            "CONCAT_WS",
        };

        private static readonly HashSet<string> IllegalFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ISNULL",
            "COALESCE",
        };

        public NullHandlingForConcatRule() : base()
        {
        }

        public override void Visit(FunctionCall node)
        {
            if (!ConcatFunctions.Contains(node.FunctionName.Value))
            {
                return;
            }

            var concatParams = node.Parameters;
            int concatParamsStart = 0;
            if (string.Equals("CONCAT_WS", node.FunctionName.Value, StringComparison.OrdinalIgnoreCase))
            {
                // skipping the delimiter
                concatParamsStart = 1;
            }

            DetectIsNull(concatParams, concatParamsStart, ViolationHandlerWithMessage);
        }

        private static void DetectIsNull(IList<ScalarExpression> args, int concatParamsStart, Action<TSqlFragment, string> callback)
        {
            var cleanedArgs = ExtractExpressions(args, concatParamsStart).ToArray();

            foreach (var funcCall in ExtractFunctionCalls(cleanedArgs))
            {
                callback(funcCall.Key, funcCall.Value);
            }

            foreach (var funcCall in ExtractCoalesceCalls(cleanedArgs))
            {
                callback(funcCall.Key, funcCall.Value);
            }
        }

        private static IEnumerable<ScalarExpression> ExtractExpressions(IList<ScalarExpression> src, int start)
        {
            for (int i = start, n = src.Count; i < n; i++)
            {
                yield return ExtractExpression(src[i]);
            }
        }

        private static ScalarExpression ExtractExpression(ScalarExpression src)
        {
            while (src is ParenthesisExpression pe)
            {
                src = pe.Expression;
            }

            return src;
        }

        private static IEnumerable<KeyValuePair<TSqlFragment, string>> ExtractFunctionCalls(ScalarExpression[] args)
        {
            foreach (var arg in args)
            {
                if (arg is FunctionCall fn
                && IllegalFunctions.Contains(fn.FunctionName.Value)
                && fn.Parameters.Count > 1)
                {
                    var lastArg = fn.Parameters[fn.Parameters.Count - 1];
                    if (ExtractExpression(lastArg) is StringLiteral str && string.IsNullOrEmpty(str.Value))
                    {
                        yield return new KeyValuePair<TSqlFragment, string>(lastArg, fn.FunctionName.Value);
                    }
                }
            }
        }

        private static IEnumerable<KeyValuePair<TSqlFragment, string>> ExtractCoalesceCalls(ScalarExpression[] args)
        {
            const string funcName = "COALESCE";
            foreach (var arg in args)
            {
                if (arg is CoalesceExpression fn
                && fn.Expressions.Count > 1)
                {
                    var lastArg = fn.Expressions[fn.Expressions.Count - 1];
                    if (ExtractExpression(lastArg) is StringLiteral str && string.IsNullOrEmpty(str.Value))
                    {
                        yield return new KeyValuePair<TSqlFragment, string>(lastArg, funcName);
                    }
                }
            }
        }
    }
}
