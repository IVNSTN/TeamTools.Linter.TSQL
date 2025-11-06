using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class StringRegexMatchVisitor
    {
        private readonly Regex regexToMatch;
        private readonly Action<int, string, string> callback;
        private readonly IList<TSqlTokenType> stringTokens = new List<TSqlTokenType>();

        public StringRegexMatchVisitor(Regex regex, bool includingComments, Action<int, string, string> callback)
        {
            this.callback = callback;
            regexToMatch = regex;

            stringTokens.Add(TSqlTokenType.AsciiStringLiteral);
            stringTokens.Add(TSqlTokenType.AsciiStringOrQuotedIdentifier);
            stringTokens.Add(TSqlTokenType.UnicodeStringLiteral);

            if (includingComments)
            {
                stringTokens.Add(TSqlTokenType.SingleLineComment);
                stringTokens.Add(TSqlTokenType.MultilineComment);
            }
        }

        public void DetectMatch(TSqlScript node)
        {
            string foundMatch;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;
            for (int i = start; i <= end; i++)
            {
                if (stringTokens.Contains(node.ScriptTokenStream[i].TokenType))
                {
                    foundMatch = DetectMatch(node.ScriptTokenStream[i].Text);
                    if (!string.IsNullOrEmpty(foundMatch))
                    {
                        callback?.Invoke(i, foundMatch, node.ScriptTokenStream[i].Text);
                    }
                }
            }
        }

        protected string DetectMatch(string src)
        {
            if (string.IsNullOrWhiteSpace(src))
            {
                return "";
            }

            var m = regexToMatch.Matches(src);
            if (m.Count == 0)
            {
                return "";
            }

            return m[0].Groups["output"].Value;
        }
    }
}
