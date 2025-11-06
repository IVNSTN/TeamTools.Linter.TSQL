using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0201", "KEYWORD_UPPER")]
    internal sealed class KeywordUppercaseRule : AbstractRule, ICodeFixProvider
    {
        private static readonly ICollection<string> Keywords = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        // TODO : consolidate all the metadata in resource file
        // but currently it contains all the possible keywords including ODBC
        // and words that can be legally used as identifiers - this is too much.
        static KeywordUppercaseRule()
        {
            Keywords.Add("TRY");
            Keywords.Add("CATCH");
            Keywords.Add("THROW");
        }

        public KeywordUppercaseRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var badKeywords = node.ScriptTokenStream
                .Where(token => token.IsKeyword()
                    || (token.TokenType == TSqlTokenType.Identifier && Keywords.Contains(token.Text)))
                .Where(token => !token.Text.Equals(token.Text.ToUpperInvariant()))
                .ToList();

            foreach (var keyword in badKeywords)
            {
                HandleLineError(keyword.Line, keyword.Column);
            }
        }
    }
}
