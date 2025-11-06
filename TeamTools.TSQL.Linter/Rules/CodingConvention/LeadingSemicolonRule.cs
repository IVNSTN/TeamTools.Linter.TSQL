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

        public override void Visit(TSqlScript node)
        {
            int lastNonWhiteSpaceLine = -1;
            int lastWhitespaceLine = -1;
            int semicolonLine = -1;
            int semicolonTokenIndex = -1;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;

            for (int i = start; i <= end; i++)
            {
                if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.WhiteSpace)
                {
                    lastWhitespaceLine = node.ScriptTokenStream[i].Line;
                }
                else if ((node.ScriptTokenStream[i].TokenType == TSqlTokenType.SingleLineComment)
                || (node.ScriptTokenStream[i].TokenType == TSqlTokenType.MultilineComment))
                {
                    if (lastNonWhiteSpaceLine == -1)
                    {
                        lastNonWhiteSpaceLine = node.ScriptTokenStream[i].Line
                             + ((node.ScriptTokenStream[i].Text?.LineCount() ?? 1) - 1);
                    }
                }
                else if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.Semicolon)
                {
                    semicolonLine = node.ScriptTokenStream[i].Line;
                    semicolonTokenIndex = i;
                }
                else if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.EndOfFile)
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

                    lastNonWhiteSpaceLine = node.ScriptTokenStream[i].Line
                        + ((node.ScriptTokenStream[i].Text?.LineCount() ?? 1) - 1);
                }
            }
        }
    }
}
