using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0244", "ML_COMMENT_OPEN_CLOSE_TAG_ALIGNED")]
    internal sealed class MultilineCommentClosingTagAlignRule : AbstractRule
    {
        private static readonly Regex FirstLineIsEmpty = new Regex("^/[*]+\\s*$", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex LastLineIsEmpty = new Regex("^\\s*[*]+/$", RegexOptions.Compiled | RegexOptions.Singleline);

        public MultilineCommentClosingTagAlignRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node) => ValidateCommentsAlignment(node.ScriptTokenStream, ViolationHandlerPerLine);

        private static void ValidateCommentsAlignment(IList<TSqlParserToken> tokens, Action<int, int, string> callback)
        {
            int n = tokens.Count;
            for (int i = 0; i < n; i++)
            {
                var token = tokens[i];
                if (token.TokenType != TSqlTokenType.MultilineComment)
                {
                    continue;
                }

                string commentText = token.Text;
                // TODO : less string manufacturing
                string[] lines = commentText.Split(Environment.NewLine);
                int lineCount = lines.Length;

                if (lineCount <= 1)
                {
                    // one-line ignored
                    continue;
                }

                var lastLine = lines[lineCount - 1];
                // TODO : no need in regex here
                bool lastLineIsEmpty = LastLineIsEmpty.IsMatch(lastLine);

                if (FirstLineIsEmpty.IsMatch(lines[0]))
                {
                    // block style
                    if (lastLineIsEmpty && (lastLine.Length - 1 == token.Column))
                    {
                        continue;
                    }
                }
                else
                {
                    // brick style
                    if (!lastLineIsEmpty && (lastLine.Length - 1 >= token.Column))
                    {
                        continue;
                    }
                }

                callback(token.Line + lineCount - 1, lastLine.Length, default);
            }
        }
    }
}
