using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void Visit(TSqlScript node)
        {
            ValidateCommentsAlignment(
                node.ScriptTokenStream.Where(t => t.TokenType == TSqlTokenType.MultilineComment),
                (line, col) => HandleLineError(line, col));
        }

        private static void ValidateCommentsAlignment(IEnumerable<TSqlParserToken> tokens, Action<int, int> callback)
        {
            foreach (var token in tokens)
            {
                string commentText = token.Text;
                string[] lines = commentText.Split(Environment.NewLine);
                int lineCount = lines.Length;

                if (lineCount <= 1)
                {
                    // one-line ignored
                    continue;
                }

                bool lastLineIsEmpty = LastLineIsEmpty.IsMatch(lines[lines.Length - 1]);

                if (FirstLineIsEmpty.IsMatch(lines[0]))
                {
                    // block style
                    if (lastLineIsEmpty && (lines[lines.Length - 1].Length - 1 == token.Column))
                    {
                        continue;
                    }
                }
                else
                {
                    // brick style
                    if (!lastLineIsEmpty && (lines[lines.Length - 1].Length - 1 >= token.Column))
                    {
                        continue;
                    }
                }

                callback(token.Line + lineCount - 1, lines[lines.Length - 1].Length);
            }
        }
    }
}
