using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0251", "DANGLING_SEMICOLON")]
    internal sealed class DanglingSemicolonRule : AbstractRule
    {
        public DanglingSemicolonRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            int lastNonWhiteSpaceLine = -1;
            int semicolonLine = -1;
            int semicolonTokenIndex = -1;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;

            for (int i = start; i < end; i++)
            {
                var token = node.ScriptTokenStream[i];
                int tokenLineNumber = token.Line;
                switch (token.TokenType)
                {
                    case TSqlTokenType.WhiteSpace:
                        {
                            if (lastNonWhiteSpaceLine == -1)
                            {
                                // FIXME: it doesnot report if dangling semicolon is the very first symbol in a file
                                lastNonWhiteSpaceLine = tokenLineNumber;
                            }

                            break;
                        }

                    case TSqlTokenType.Semicolon:
                        {
                            semicolonLine = tokenLineNumber;
                            semicolonTokenIndex = i;
                            break;
                        }

                    case TSqlTokenType.EndOfFile:
                    case TSqlTokenType.None:
                        // ignoring
                        break;
                    default:
                        {
                            if ((semicolonLine >= 0) && (lastNonWhiteSpaceLine >= 0)
                            && (semicolonLine > lastNonWhiteSpaceLine) && (semicolonLine < tokenLineNumber))
                            {
                                HandleTokenError(node.ScriptTokenStream[semicolonTokenIndex]);
                                semicolonLine = -1;
                                semicolonTokenIndex = -1;
                            }

                            lastNonWhiteSpaceLine = tokenLineNumber
                                // for multiline strings and so on
                                + (token.Text.LineCount() - 1);
                            break;
                        }
                }
            }

            // if nothing was after semicolon
            if ((semicolonLine >= 0) && (lastNonWhiteSpaceLine < semicolonLine))
            {
                HandleTokenError(node.ScriptTokenStream[semicolonTokenIndex]);
            }
        }
    }
}
