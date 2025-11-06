using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

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

            DoValidateFormatArgCount(node.Parameters[0], node.Parameters.Skip(1).ToList());
        }

        public override void Visit(RaiseErrorStatement node) => DoValidateFormatArgCount(node.FirstParameter, node.OptionalParameters);

        private static bool IsValidArgCount(string template, IList<ScalarExpression> args)
            => args.Count == FormatMessageWildcardExtractor.ExtractWildcards(template).Count();

        private void DoValidateFormatArgCount(ScalarExpression input, IList<ScalarExpression> args)
        {
            if (input is StringLiteral str
            && !string.IsNullOrEmpty(str.Value)
            && !IsValidArgCount(str.Value, args))
            {
                HandleNodeError(input);
            }
        }
    }
}
