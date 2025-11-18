using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0241", "SL_COMMENT_TRAILING_DASHES")]
    internal sealed class RedundantDashAfterSingleLineCommentRule : BaseSingleLineCommentValidationRule
    {
        public RedundantDashAfterSingleLineCommentRule() : base()
        {
        }

        protected override bool IsValidCommentFormat(string text)
        {
            for (int i = text.Length - 1; i > 2; i--)
            {
                var c = text[i];
                if (c == '-')
                {
                    return false;
                }
                else if (c != ' ')
                {
                    return true;
                }
            }

            return true;
        }
    }
}
