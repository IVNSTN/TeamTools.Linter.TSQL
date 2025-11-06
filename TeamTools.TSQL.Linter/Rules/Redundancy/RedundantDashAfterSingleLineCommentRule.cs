using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0241", "SL_COMMENT_TRAILING_DASHES")]
    internal sealed class RedundantDashAfterSingleLineCommentRule : BaseSingleLineCommentValidationRule
    {
        private static readonly Regex DetectTrailingDash = new Regex("^--.+[-]{1,}$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        public RedundantDashAfterSingleLineCommentRule() : base()
        {
        }

        protected override bool IsValidCommentFormat(string text) => !DetectTrailingDash.IsMatch(text.Replace(" ", ""));
    }
}
