using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0240", "SL_COMMENT_LEADING_DASHES")]
    internal sealed class RedundantDashBeforeSingleLineCommentRule : BaseSingleLineCommentValidationRule
    {
        public RedundantDashBeforeSingleLineCommentRule() : base()
        {
        }

        protected override bool IsValidCommentFormat(string text)
        {
            for (int i = 2, n = text.Length; i < n; i++)
            {
                var c = text[i];
                if (c == '-')
                {
                    return false;
                }
                else if (!char.IsWhiteSpace(c))
                {
                    return true;
                }
            }

            return true;
        }
    }
}
