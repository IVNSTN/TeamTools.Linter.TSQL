using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FL0308", "EOF_GO")]
    internal sealed class EndOfFileGoRule : AbstractRule
    {
        public EndOfFileGoRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            TSqlParserToken token = default;

            for (int i = node.ScriptTokenStream.Count - 1; i >= 0; i--)
            {
                token = node.ScriptTokenStream[i];
                if (!(token.TokenType == TSqlTokenType.WhiteSpace
                   || token.TokenType == TSqlTokenType.EndOfFile
                   || token.TokenType == TSqlTokenType.Semicolon))
                {
                    break;
                }
            }

            if (token is null
            || token.TokenType == TSqlTokenType.EndOfFile
            || token.TokenType == TSqlTokenType.WhiteSpace)
            {
                // empty file
                return;
            }

            if (token.TokenType == TSqlTokenType.Go)
            {
                // GO found
                return;
            }

            HandleLineError(token.Line, 1);
        }
    }
}
