using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0243", "ML_COMMENT_CONTENT_ALIGN")]
    internal sealed class MultilineCommentOpenCloseTagAlignRule : AbstractRule
    {
        public MultilineCommentOpenCloseTagAlignRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            for (int i = node.ScriptTokenStream.Count - 1; i >= 0; i--)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.MultilineComment)
                {
                    ValidateCommentFormat(token);
                }
            }
        }

        private void ValidateCommentFormat(TSqlParserToken commentToken)
        {
            // TODO : it should be configurable
            const int tabSize = 4;

            string comment = commentToken.Text;
            int n = comment.Length;
            // because column number here is the first '/*' position, not the offset
            int minOffset = commentToken.Column > 0 ? commentToken.Column - 1 : 0;

            int currentLineOffset = 0;
            int line = 0;
            bool lastWasSpace = false;

            for (int i = 0; i < n; i++)
            {
                var c = comment[i];
                if (c == ' ')
                {
                    currentLineOffset++;
                    lastWasSpace = true;
                }
                else if (c == '\t')
                {
                    currentLineOffset += tabSize;
                    lastWasSpace = true;
                }
                else if (c == '\r')
                {
                    if (line > 0 && (currentLineOffset > 0 || !lastWasSpace) && currentLineOffset < minOffset)
                    {
                        HandleLineError(commentToken.Line + line, currentLineOffset);
                        // one warning per comment is enough
                        break;
                    }

                    currentLineOffset = 0;
                    line++;
                    lastWasSpace = true;

                    if (i + 1 < n && comment[i + 1] == '\n')
                    {
                        i++;
                    }
                }
                else if (c == '\n')
                {
                    if (line > 0 && (currentLineOffset > 0 || !lastWasSpace) && currentLineOffset < minOffset)
                    {
                        HandleLineError(commentToken.Line + line, currentLineOffset);
                        // one warning per comment is enough
                        break;
                    }

                    currentLineOffset = 0;
                    line++;
                    lastWasSpace = true;
                }
                else
                {
                    lastWasSpace = false;
                }
            }
        }
    }
}
