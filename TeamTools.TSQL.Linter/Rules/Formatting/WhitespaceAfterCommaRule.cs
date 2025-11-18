using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0231", "WHITESPACE_AFTER_COMMA")]
    internal sealed class WhitespaceAfterCommaRule : AbstractRule
    {
        public WhitespaceAfterCommaRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            TSqlParserToken lastCommaToken = null;
            int spaceLengthAfterComma = 0;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;

            for (int i = start; i < end; i++)
            {
                var token = node.ScriptTokenStream[i];
                switch (token.TokenType)
                {
                    case TSqlTokenType.Comma:
                        {
                            lastCommaToken = token;
                            spaceLengthAfterComma = 0;

                            break;
                        }

                    case TSqlTokenType.WhiteSpace:
                        {
                            if (lastCommaToken != null && !string.IsNullOrEmpty(token.Text))
                            {
                                // it is either whitespaces or linebreaks
                                if (token.Text[0] == ' ')
                                {
                                    spaceLengthAfterComma = spaceLengthAfterComma + token.Text.Length;
                                }
                                else
                                {
                                    // will count line diff later
                                    // and we don't care about trailing spaces - there is a dedicated rule for this
                                    spaceLengthAfterComma = 0;
                                }
                            }

                            break;
                        }

                    default:
                        {
                            if (lastCommaToken != null
                            && ((spaceLengthAfterComma != 1 && token.Line == lastCommaToken.Line)
                            || (token.Line - lastCommaToken.Line) > 1))
                            {
                                HandleTokenError(lastCommaToken);
                            }

                            lastCommaToken = null;
                            spaceLengthAfterComma = 0;
                            break;
                        }
                }
            }
        }
    }
}
