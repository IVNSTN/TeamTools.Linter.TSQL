using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0239", "SL_COMMENT_LEADING_SPACE")]
    internal sealed class SingleSpaceAfterSingleLineCommentBeginRule : BaseSingleLineCommentValidationRule, ICommentAnalyzer
    {
        private static readonly char[] TrimmedChars = new char[] { '-', ' ' };
        private static readonly string EmptyComment = "--";
        private static readonly Regex SingleSpaceRegex = new Regex(
            "^[-]+(\\s){1}[^\\s]+",
            RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly List<string> specialComments = new List<string>();

        public SingleSpaceAfterSingleLineCommentBeginRule() : base()
        {
        }

        public void LoadSpecialCommentPrefixes(ICollection<string> values)
        {
            specialComments.Clear();
            specialComments.AddRange(values);
        }

        protected override bool IsValidCommentFormat(string comment)
        {
            if (EmptyComment.Equals(comment))
            {
                // empty comment for formatting and so on
                return true;
            }

            if (IsSpecialComment(comment))
            {
                // special comments may violate our rules
                return true;
            }

            return SingleSpaceRegex.IsMatch(comment);
        }

        private bool IsSpecialComment(string comment)
        => specialComments.Any(prefix => comment
            .TrimStart(TrimmedChars)
            .StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
