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
        private static readonly ICollection<string> ConcatFunctions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        private static readonly ICollection<string> IllegalFunctions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        static NullHandlingForConcatRule()
        {
            ConcatFunctions.Add("CONCAT");
            ConcatFunctions.Add("CONCAT_WS");

            IllegalFunctions.Add("ISNULL");
            IllegalFunctions.Add("COALESCE");
        }

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
            if (string.Equals("CONCAT_WS", node.FunctionName.Value, StringComparison.OrdinalIgnoreCase))
            {
                // skipping the delimiter
                concatParams = concatParams.Skip(1).ToList();
            }

            DetectIsNull(concatParams, HandleNodeError);
        }

        private static void DetectIsNull(IList<ScalarExpression> args, Action<TSqlFragment, string> callback)
        {
            var cleanedArgs = args.Select(a => ExtractExpression(a));
            var funcCalls = ExtractFunctionCalls(cleanedArgs)
                .Union(ExtractCoalesceCalls(cleanedArgs));

            foreach (var funcCall in funcCalls)
            {
                callback(funcCall.Key, funcCall.Value);
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

        private static IEnumerable<KeyValuePair<TSqlFragment, string>> ExtractFunctionCalls(IEnumerable<ScalarExpression> args)
        {
            return args
                .OfType<FunctionCall>()
                .Where(fn => IllegalFunctions.Contains(fn.FunctionName.Value))
                .Where(fn => fn.Parameters.Count > 1)
                .Where(fn => ExtractExpression(fn.Parameters[fn.Parameters.Count - 1]) is StringLiteral str && string.IsNullOrEmpty(str.Value))
                .Select(fn => new KeyValuePair<TSqlFragment, string>(fn.Parameters[fn.Parameters.Count - 1], fn.FunctionName.Value));
        }

        private static IEnumerable<KeyValuePair<TSqlFragment, string>> ExtractCoalesceCalls(IEnumerable<ScalarExpression> args)
        {
            return args
                .OfType<CoalesceExpression>()
                .Where(fn => fn.Expressions.Count > 1)
                .Where(fn => ExtractExpression(fn.Expressions[fn.Expressions.Count - 1]) is StringLiteral str && string.IsNullOrEmpty(str.Value))
                .Select(fn => new KeyValuePair<TSqlFragment, string>(fn.Expressions[fn.Expressions.Count - 1], "COALESCE"));
        }
    }
}
