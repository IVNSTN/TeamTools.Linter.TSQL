using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0245", "MANY_SL_COMMENTS_TO_ML")]
    internal sealed class MultipleSingleLineCommentsToMultilineCommentRule : AbstractRule, ICommentAnalyzer
    {
        private const int MaxCommentInARow = 2;
        private static readonly char[] TrimmedChars = new char[] { '-', ' ' };
        private string[] specialComments;

        public MultipleSingleLineCommentsToMultilineCommentRule() : base()
        {
        }

        public void LoadSpecialCommentPrefixes(ICollection<string> values)
        {
            specialComments = values.ToArray();
        }

        protected override void ValidateScript(TSqlScript node)
        {
            int commentCount = 0;
            int lastCommentTokenIndex = -1;
            bool lastWasMultiline = false;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;

            for (int i = start; i < end; i++)
            {
                var token = node.ScriptTokenStream[i];
                switch (token.TokenType)
                {
                    case TSqlTokenType.WhiteSpace:
                        // do nothing
                        break;
                    case TSqlTokenType.SingleLineComment:
                        {
                            if (IsSpecialComment(token.Text))
                            {
                                // ignoring special comments which may loose their magic if combined with other comments
                                break;
                            }

                            commentCount++;
                            lastCommentTokenIndex = i;
                            lastWasMultiline = false;
                            break;
                        }

                    case TSqlTokenType.MultilineComment:
                        {
                            if (lastWasMultiline)
                            {
                                break;
                            }

                            commentCount += MaxCommentInARow;
                            if (lastCommentTokenIndex == -1)
                            {
                                lastCommentTokenIndex = i;
                            }

                            lastWasMultiline = true;
                            break;
                        }

                    default:
                        {
                            if ((commentCount > MaxCommentInARow) && (lastCommentTokenIndex >= 0))
                            {
                                HandleTokenError(node.ScriptTokenStream[lastCommentTokenIndex]);
                            }

                            commentCount = 0;
                            lastCommentTokenIndex = -1;
                            break;
                        }
                }
            }
        }

        // TODO : very similar to SingleSpaceAfterSingleLineCommentBeginRule.IsSpecialComment
        private bool IsSpecialComment(string comment)
        {
            comment = comment.TrimStart(TrimmedChars);

            foreach (var prefix in specialComments)
            {
                if (comment.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
