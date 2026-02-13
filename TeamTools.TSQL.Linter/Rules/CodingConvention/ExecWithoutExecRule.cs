using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0810", "EXEC_WITHOUT_EXEC")]
    internal sealed class ExecWithoutExecRule : AbstractRule
    {
        public ExecWithoutExecRule() : base()
        {
        }

        public override void Visit(ExecuteStatement node)
        {
            int i = node.FirstTokenIndex;
            int n = node.LastTokenIndex;

            if (i == n && i >= 0)
            {
                var statementText = node.ScriptTokenStream[i].Text;

                // TODO : shouldn't it detect longer sequences of such symbols?
                if (statementText.Length == 1
                && InvisibleCharDetector.LocateInvisibleChar(statementText, out var _) == 0)
                {
                    // this is an invisible unicode symbol which is supposed to be detected by a separate rule
                    return;
                }
            }

            while (i < n && ScriptDomExtension.IsSkippableTokens(node.ScriptTokenStream[i].TokenType))
            {
                i++;
            }

            var token = node.ScriptTokenStream[i];

            if (token.TokenType != TSqlTokenType.Exec
            && token.TokenType != TSqlTokenType.Execute)
            {
                HandleTokenError(token);
            }
        }
    }
}
