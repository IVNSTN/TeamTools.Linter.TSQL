using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0932", "TODO_COMMENT")]
    internal sealed class ToDoCommentsRule : AbstractRule
    {
        private static readonly ICollection<string> SpecialComments = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
        {
           "TODO",
           "TBD",
           "FIXME",
           "HACK",
           "REMOVEONRELEASE",
        };

        private static readonly Regex CommentPrefixRegex = new Regex(
            @"([\-]{2,}|/[*]{1,})\s*(?<prefix>[a-zA-Z]+).*",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);

        public ToDoCommentsRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var comments = node.ScriptTokenStream
                .Where(token => token.TokenType == TSqlTokenType.MultilineComment || token.TokenType == TSqlTokenType.SingleLineComment);

            foreach (var comment in DetectSpecialComments(comments))
            {
                HandleLineError(comment.Value.Line, comment.Value.Column, comment.Key);
            }
        }

        private static IEnumerable<KeyValuePair<string, TSqlParserToken>> DetectSpecialComments(IEnumerable<TSqlParserToken> comments)
        {
            foreach (var comment in comments)
            {
                var m = CommentPrefixRegex.Matches(comment.Text);
                if (m.Count == 0)
                {
                    continue;
                }

                string prefix = m[0].Groups["prefix"].Value;
                if (SpecialComments.Contains(prefix))
                {
                    yield return new KeyValuePair<string, TSqlParserToken>(prefix.ToUpperInvariant(), comment);
                }
            }
        }
    }
}
