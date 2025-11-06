using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0240", "SL_COMMENT_LEADING_DASHES")]
    internal sealed class RedundantDashBeforeSingleLineCommentRule : BaseSingleLineCommentValidationRule
    {
        private static readonly Regex DetectExtraLeadingDash = new Regex("^[-]{3,}", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        public RedundantDashBeforeSingleLineCommentRule() : base()
        {
        }

        protected override bool IsValidCommentFormat(string text) => !DetectExtraLeadingDash.IsMatch(text.Replace(" ", ""));
    }
}
