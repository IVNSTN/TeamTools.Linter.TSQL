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

        public override void Visit(TSqlScript node)
        {
            int lastNonWhiteSpaceLine = -1;
            int semicolonLine = -1;
            int semicolonTokenIndex = -1;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;

            for (int i = start; i <= end; i++)
            {
                int tokenLineNumber = node.ScriptTokenStream[i].Line;
                switch (node.ScriptTokenStream[i].TokenType)
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
                                + (node.ScriptTokenStream[i].Text.LineCount() - 1);
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
