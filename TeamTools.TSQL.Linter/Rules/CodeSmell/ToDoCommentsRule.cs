using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0932", "TODO_COMMENT")]
    internal sealed class ToDoCommentsRule : AbstractRule
    {
        private static readonly HashSet<string> ConventionalComments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
           "TODO",
           "TBD",
           "FIXME",
           "HACK",
           "REMOVEONRELEASE",
        };

        private static readonly int MinCommentLength = 5; // e.g. "--TBD"

        private static readonly Regex CommentPrefixRegex = new Regex(
            @"^(?:[/*-]{2,}\s*)(?<prefix>[a-zA-Z]{3,20})",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline);

        public ToDoCommentsRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            for (int i = node.ScriptTokenStream.Count - 1; i >= 0; i--)
            {
                var token = node.ScriptTokenStream[i];

                if (token.TokenType != TSqlTokenType.MultilineComment
                && token.TokenType != TSqlTokenType.SingleLineComment)
                {
                    continue;
                }

                if (token.Text.Length >= MinCommentLength
                && DetectSpecialComments(token.Text, out string conventionalPrefix))
                {
                    HandleTokenError(token, conventionalPrefix);
                }
            }
        }

        private static bool DetectSpecialComments(string commentText, out string conventionalPrefix)
        {
            var m = CommentPrefixRegex.Match(commentText);
            conventionalPrefix = m?.Groups["prefix"].Value;

            return !string.IsNullOrEmpty(conventionalPrefix)
                && ConventionalComments.Contains(conventionalPrefix);
        }
    }
}
