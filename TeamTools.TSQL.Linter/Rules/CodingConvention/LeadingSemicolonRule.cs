using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0252", "LEADING_SEMICOLON")]
    internal class LeadingSemicolonRule : AbstractRule
    {
        public LeadingSemicolonRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            int lastNonWhiteSpaceLine = -1;
            int lastWhitespaceLine = -1;
            int semicolonLine = -1;
            int semicolonTokenIndex = -1;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;

            for (int i = start; i < end; i++)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.WhiteSpace)
                {
                    lastWhitespaceLine = token.Line;
                }
                else if ((token.TokenType == TSqlTokenType.SingleLineComment)
                || (token.TokenType == TSqlTokenType.MultilineComment))
                {
                    if (lastNonWhiteSpaceLine == -1)
                    {
                        lastNonWhiteSpaceLine = token.Line
                             + ((token.Text?.LineCount() ?? 1) - 1);
                    }
                }
                else if (token.TokenType == TSqlTokenType.Semicolon)
                {
                    semicolonLine = token.Line;
                    semicolonTokenIndex = i;
                }
                else if (token.TokenType == TSqlTokenType.EndOfFile)
                {
                    break;
                }
                else
                {
                    // semicolon is somewhere after real code and there is a whitespace between them
                    if ((semicolonLine >= 0) && (lastNonWhiteSpaceLine >= 0) && (semicolonLine > lastNonWhiteSpaceLine)
                    && lastWhitespaceLine > lastNonWhiteSpaceLine && lastWhitespaceLine <= semicolonLine)
                    {
                        HandleTokenError(node.ScriptTokenStream[semicolonTokenIndex]);
                        semicolonLine = -1;
                        semicolonTokenIndex = -1;
                    }

                    lastNonWhiteSpaceLine = token.Line
                        + ((token.Text?.LineCount() ?? 1) - 1);
                }
            }
        }
    }
}
