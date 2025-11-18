using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0248", "REDUNDANT_SEMICOLON")]
    internal sealed class RedundantSemicolonRule : AbstractRule
    {
        public RedundantSemicolonRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            TSqlParserToken lastSemicolonToken = null;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;
            bool alreadyReported = false;

            for (int i = start; i < end; i++)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.Semicolon)
                {
                    if (lastSemicolonToken != null)
                    {
                        if (!alreadyReported)
                        {
                            HandleTokenError(lastSemicolonToken);

                            // little less reporting on ;;;;;;
                            alreadyReported = true;
                        }
                    }
                    else
                    {
                        alreadyReported = false;
                        lastSemicolonToken = token;
                    }
                }
                else if (!ScriptDomExtension.IsNonStatementToken(token.TokenType))
                {
                    lastSemicolonToken = null;
                }
            }
        }
    }
}
