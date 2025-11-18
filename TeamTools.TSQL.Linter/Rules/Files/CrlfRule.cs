using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FL0303", "CRLF")]
    internal sealed class CrlfRule : AbstractRule
    {
        public CrlfRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            int n = node.ScriptTokenStream.Count;
            for (int i = 0; i < n; i++)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.WhiteSpace
                && token.Text.Length != 0
                && !CheckLineEndings(token.Text))
                {
                    // Pointing specifically to the very line end may break delivery to SonarQube
                    HandleLineError(token.Line, 0);
                }
            }
        }

        private static bool CheckLineEndings(string scriptText)
        {
            bool carriageReturn = false;

            const char carriageReturnChar = '\r';
            const char newlineChar = '\n';

            int n = scriptText.Length;
            for (int i = 0; i < n; i++)
            {
                var c = scriptText[i];
                if (c == newlineChar)
                {
                    if (!carriageReturn)
                    {
                        // LF without preceding CR
                        return false;
                    }

                    // CRLF is complete
                    carriageReturn = false;
                }
                else
                {
                    if (carriageReturn)
                    {
                        // multiple CR in a row
                        return false;
                    }

                    if (c == carriageReturnChar)
                    {
                        // CRLF candidate started
                        carriageReturn = true;
                    }
                }
            }

            // In case if it was single/trailing CR
            return !carriageReturn;
        }
    }
}
