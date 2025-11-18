using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0201", "KEYWORD_UPPER")]
    internal sealed class KeywordUppercaseRule : AbstractRule, ICodeFixProvider
    {
        // TODO : consolidate all the metadata in resource file
        // but currently it contains all the possible keywords including ODBC
        // and words that can be legally used as identifiers - this is too much.
        private static readonly HashSet<string> Keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
           "CATCH",
           "THROW",
           "TRY",
        };

        private static readonly HashSet<TSqlTokenType> KeywordTokens;

        static KeywordUppercaseRule()
        {
            // filter copied from ScriptDom sources
            KeywordTokens = new HashSet<TSqlTokenType>(
                Enum.GetValues(typeof(TSqlTokenType))
                    .OfType<TSqlTokenType>()
                    .Where(tokenType => tokenType > TSqlTokenType.EndOfFile && tokenType < TSqlTokenType.Bang));

            KeywordTokens.Add(TSqlTokenType.Go);
        }

        public KeywordUppercaseRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            for (int i = node.ScriptTokenStream.Count - 1; i >= 0; i--)
            {
                var token = node.ScriptTokenStream[i];

                if (!string.IsNullOrEmpty(token.Text) && IsKeyword(token))
                {
                    if (!token.Text.IsUpperCase())
                    {
                        HandleTokenError(token);
                    }
                }
            }
        }

        private static bool IsKeyword(TSqlParserToken token)
        {
            var tp = token.TokenType;
            return (tp > TSqlTokenType.EndOfFile && tp < TSqlTokenType.Bang)
                || (tp == TSqlTokenType.Identifier && token.Text.Length >= 3 && Keywords.Contains(token.Text));
        }
    }
}
