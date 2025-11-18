using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [Obsolete("Looks like a dup of " + nameof(RedundantFunctionArgumentRule))]
    [RuleIdentity("FA0911", "FORMAT_ARG_COUNT_MISMATCH")]
    internal sealed class FormatArgumentCountMismatchRule : AbstractRule
    {
        public FormatArgumentCountMismatchRule() : base()
        {
        }

        // TODO : ExpressionEvaluator/FormatMessage handler should fully support this case.
        public override void Visit(FunctionCall node)
        {
            if (!string.Equals(node.FunctionName.Value, "FORMATMESSAGE", StringComparison.OrdinalIgnoreCase)
            || node.Parameters.Count < 2)
            {
                return;
            }

            // First arg is the message template thus ignoring it
            DoValidateFormatArgCount(node.Parameters[0], node.Parameters.Count - 1);
        }

        public override void Visit(RaiseErrorStatement node) => DoValidateFormatArgCount(node.FirstParameter, node.OptionalParameters.Count);

        private static bool IsValidArgCount(string template, int argCount)
            => argCount == FormatMessageWildcardExtractor.CountWildcards(template);

        private void DoValidateFormatArgCount(ScalarExpression input, int argCount)
        {
            if (input is StringLiteral str
            && !string.IsNullOrEmpty(str.Value)
            && !IsValidArgCount(str.Value, argCount))
            {
                HandleNodeError(input);
            }
        }
    }
}
