using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0783", "DECLARE_AS")]
    internal sealed class DeclareAsTypeRule : AbstractRule
    {
        public DeclareAsTypeRule() : base()
        {
        }

        public override void Visit(DeclareVariableElement node)
        {
            for (int i = node.DataType.FirstTokenIndex - 1, start = node.VariableName.LastTokenIndex; i >= start; i--)
            {
                var token = node.ScriptTokenStream[i];

                if (token.TokenType == TSqlTokenType.As)
                {
                    HandleTokenError(token);
                }
            }
        }
    }
}
