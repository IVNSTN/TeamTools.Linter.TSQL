using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Text.RegularExpressions;

namespace TeamTools.TSQL.Linter.Routines
{
    internal static class StringRegexMatchVisitor
    {
        public static void DetectMatch(TSqlScript node, Regex regexToMatch, Action<TSqlParserToken, string> callback, int minLength = 1)
        {
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;
            for (int i = start; i < end; i++)
            {
                var token = node.ScriptTokenStream[i];

                if (!string.IsNullOrEmpty(token.Text)
                && token.Text.Length >= minLength
                && HasText(token.TokenType))
                {
                    string foundMatch = regexToMatch.Match(token.Text)?.Groups["output"].Value;
                    if (!string.IsNullOrEmpty(foundMatch))
                    {
                        callback(token, foundMatch);
                    }
                }
            }
        }

        public static void DetectMatch(TSqlScript node, char symbolToFind, Action<TSqlParserToken> callback, int minLength = 1)
        {
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;
            for (int i = start; i < end; i++)
            {
                var token = node.ScriptTokenStream[i];

                if (!string.IsNullOrEmpty(token.Text)
                && token.Text.Length >= minLength
                && HasText(token.TokenType))
                {
                    if (token.Text.Contains(symbolToFind))
                    {
                        callback(token);
                    }
                }
            }
        }

        private static bool HasText(TSqlTokenType token)
        {
            return token == TSqlTokenType.AsciiStringLiteral
                || token == TSqlTokenType.AsciiStringOrQuotedIdentifier
                || token == TSqlTokenType.UnicodeStringLiteral
                || token == TSqlTokenType.SingleLineComment
                || token == TSqlTokenType.MultilineComment;
        }
    }
}
