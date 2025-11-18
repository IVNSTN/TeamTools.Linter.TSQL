using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0986", "FORMAT_INVALID_WILDCARD")]
    internal sealed class FormatMessageInvalidWildcardRule : AbstractRule
    {
        private static readonly Regex BadWildcardPattern = new Regex(
            "(?<wildcard>(?<!%)%(?<prefix>[+#-])?(?<size>[\\d]+)?(?<type>[^sxoiduSXOIDU%]))",
            RegexOptions.Multiline | RegexOptions.Compiled);

        public FormatMessageInvalidWildcardRule() : base()
        {
        }

        public override void Visit(RaiseErrorStatement node) => ValidateFormatMessageText(node.FirstParameter);

        public override void Visit(FunctionCall node)
        {
            if (!string.Equals(node.FunctionName.Value, "FORMATMESSAGE", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (node.Parameters.Count < 2)
            {
                return;
            }

            ValidateFormatMessageText(node.Parameters[0]);
        }

        private static string ValidateFormatWildcards(string template)
        {
            return BadWildcardPattern.Match(template)?.Groups["wildcard"].Value;
        }

        private void ValidateFormatMessageText(TSqlFragment msg)
        {
            // TODO : utilize evaluator
            if (!(msg is StringLiteral str))
            {
                return;
            }

            string badWildcard = ValidateFormatWildcards(str.Value);

            if (string.IsNullOrEmpty(badWildcard))
            {
                return;
            }

            HandleNodeError(msg, badWildcard);
        }
    }
}
