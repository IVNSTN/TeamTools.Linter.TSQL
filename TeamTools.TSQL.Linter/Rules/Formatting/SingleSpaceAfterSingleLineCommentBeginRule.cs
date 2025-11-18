using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0239", "SL_COMMENT_LEADING_SPACE")]
    internal sealed class SingleSpaceAfterSingleLineCommentBeginRule : BaseSingleLineCommentValidationRule, ICommentAnalyzer
    {
        private static readonly string EmptyComment = "--";

        private string[] specialComments;

        public SingleSpaceAfterSingleLineCommentBeginRule() : base()
        {
        }

        public void LoadSpecialCommentPrefixes(ICollection<string> values)
        {
            specialComments = values.ToArray();
        }

        protected override bool IsValidCommentFormat(string comment)
        {
            Debug.Assert(specialComments != null && specialComments.Length > 0, "specialComments not loaded");

            if (EmptyComment.Equals(comment))
            {
                // empty comment for formatting and so on
                return true;
            }

            int spaceCount = 0;
            int n = comment.Length;
            int i = 0;

            // moving to comment contents
            while (i < n && comment[i] == '-')
            {
                i++;
            }

            // counting leading spaces
            while (i < n && comment[i] == ' ')
            {
                i++;
                spaceCount++;
            }

            if (spaceCount == 1)
            {
                return true;
            }

            // special comments may violate rules
            return IsSpecialComment(comment.Substring(i));
        }

        private bool IsSpecialComment(string comment)
        {
            foreach (var prefix in specialComments)
            {
                if (comment.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
