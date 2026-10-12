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
            if (node.ExecuteSpecification.ExecutableEntity is ExecutableProcedureReference proc
            && proc.ProcedureReference.ProcedureReference != null
            && InvisibleCharDetector.LocateInvisibleChar(proc.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value, out var _) == 0)
            {
                // This is an invisible unicode symbol which is supposed to be detected by a separate rule.
                // The parser treats it as an identifier and parses such invisible thing as ExecuteStatement
                // with [EXEC] keyword omitted and "empty" (invisible) proc name - such cases should be ignored.
                return;
            }

            var token = LocateFirstStatementToken(node);

            // it must be EXEC or EXECUTE, not just a proc name
            if (token.TokenType != TSqlTokenType.Exec
            && token.TokenType != TSqlTokenType.Execute)
            {
                HandleTokenError(token);
            }
        }

        private static TSqlParserToken LocateFirstStatementToken(ExecuteStatement node)
        {
            int i = node.FirstTokenIndex;
            int n = node.LastTokenIndex;

            // locating the first token of the statement
            while (i < n && ScriptDomExtension.IsSkippableTokens(node.ScriptTokenStream[i].TokenType))
            {
                i++;
            }

            return node.ScriptTokenStream[i];
        }
    }
}
