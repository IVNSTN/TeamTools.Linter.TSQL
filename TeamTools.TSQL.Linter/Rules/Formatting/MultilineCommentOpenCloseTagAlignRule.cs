using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0243", "ML_COMMENT_CONTENT_ALIGN")]
    internal sealed class MultilineCommentOpenCloseTagAlignRule : AbstractRule
    {
        // ignoring empty lines
        private readonly Regex offsetRegex = new Regex("^(?<offset>\\s+)[^\\s]+", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant);

        public MultilineCommentOpenCloseTagAlignRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;

            for (int i = start; i <= end; i++)
            {
                if (node.ScriptTokenStream[i].TokenType != TSqlTokenType.MultilineComment)
                {
                    continue;
                }

                string commentText = node.ScriptTokenStream[i].Text;
                string[] lines = commentText.Split(Environment.NewLine);
                int lineCount = lines.Length;
                int minOffset = node.ScriptTokenStream[i].Column - 1;

                if (lineCount <= 1)
                {
                    // one-line ignored
                    continue;
                }

                int currentOffset;
                int n = lines.Length;

                // first line starts comment, nothing to check
                for (int j = 1; j < n; j++)
                {
                    if (string.IsNullOrEmpty(lines[j]))
                    {
                        continue;
                    }

                    var m = offsetRegex.Matches(lines[j]);

                    if (m.Count == 0)
                    {
                        currentOffset = 0;
                    }
                    else
                    {
                        currentOffset = m[0].Groups["offset"].Length;
                    }

                    if (currentOffset < minOffset)
                    {
                        HandleLineError(node.ScriptTokenStream[i].Line + j, currentOffset);
                        // one warning per comment is enough
                        break;
                    }
                }
            }
        }
    }
}
