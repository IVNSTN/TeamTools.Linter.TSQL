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
            int i = node.DataType.FirstTokenIndex - 1;
            int start = node.VariableName.LastTokenIndex;

            while (i > start && node.ScriptTokenStream[i].TokenType != TSqlTokenType.As)
            {
                i--;
            }

            if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.As)
            {
                HandleTokenError(node.ScriptTokenStream[i]);
            }
        }
    }
}
