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
