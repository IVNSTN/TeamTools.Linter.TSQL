using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0231", "WHITESPACE_AFTER_COMMA")]
    internal sealed class WhitespaceAfterCommaRule : AbstractRule
    {
        public WhitespaceAfterCommaRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            int lastCommaToken = -1;
            string whitespace = "";
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;

            for (int i = start; i <= end; i++)
            {
                switch (node.ScriptTokenStream[i].TokenType)
                {
                    case TSqlTokenType.Comma:
                        {
                            lastCommaToken = i;
                            whitespace = "";
                            break;
                        }

                    case TSqlTokenType.WhiteSpace:
                        {
                            if (lastCommaToken == -1)
                            {
                                break;
                            }

                            // linebreaks and spaces go as separate tokens
                            whitespace += node.ScriptTokenStream[i].Text;
                            break;
                        }

                    default:
                        {
                            if (lastCommaToken != -1)
                            {
                                ValidateWhitespace(
                                    whitespace,
                                    node.ScriptTokenStream[lastCommaToken].Line,
                                    node.ScriptTokenStream[lastCommaToken].Column);
                            }

                            lastCommaToken = -1;
                            break;
                        }
                }
            }
        }

        private void ValidateWhitespace(string whitespace, int commaLine, int commaColumn)
        {
            string[] lines = whitespace.Split(Environment.NewLine);

            // no spaces
            // multiple spaces in the same line before following text
            // or trailing spaces if the following text is on different line
            // or more than one line break before following text
            if (string.IsNullOrEmpty(whitespace)
            || (lines.Length == 1 && lines[0].Length > 1)
            || (lines.Length > 1 && lines[0].Length > 0)
            || (lines.Length > 2))
            {
                HandleLineError(commaLine, commaColumn);
            }
        }
    }
}
