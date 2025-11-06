using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0986", "FORMAT_INVALID_WILDCARD")]
    internal sealed class FormatMessageInvalidWildcardRule : AbstractRule
    {
        private readonly Regex badWildcardPattern = new Regex(
            "(?<wildcard>(?<!%)%(?<prefix>[+#-])?(?<size>[\\d]+)?(?<type>[^(s|x|o|i|d|u|%)]))",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public FormatMessageInvalidWildcardRule() : base()
        {
        }

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

            if (!(node.Parameters[0] is StringLiteral str))
            {
                return;
            }

            if (ValidateFormatWildcards(str.Value))
            {
                return;
            }

            HandleNodeError(node);
        }

        public override void Visit(RaiseErrorStatement node)
        {
            if (!(node.FirstParameter is StringLiteral str))
            {
                return;
            }

            if (ValidateFormatWildcards(str.Value))
            {
                return;
            }

            HandleNodeError(node);
        }

        private List<string> ExtractWildcards(string template)
        {
            return badWildcardPattern.Matches(template)
                .Select(m => m.Groups["wildcard"].Value)
                .ToList();
        }

        private bool ValidateFormatWildcards(string template)
        {
            var wildcards = ExtractWildcards(template);

            if (wildcards.Count != 0)
            {
                return false;
            }

            return true;
        }
    }
}
