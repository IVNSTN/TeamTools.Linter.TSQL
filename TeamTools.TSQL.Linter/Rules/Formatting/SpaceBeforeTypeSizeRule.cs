using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0815", "SPACE_BEFORE_SIZE")]
    internal sealed class SpaceBeforeTypeSizeRule : AbstractRule
    {
        public SpaceBeforeTypeSizeRule() : base()
        {
        }

        public override void Visit(SqlDataTypeReference node)
        {
            if (node.Parameters is null || node.Parameters.Count == 0)
            {
                // size omitted or not applicable
                return;
            }

            var i = node.Parameters[0].FirstTokenIndex - 1;
            var n = node.Name.LastTokenIndex;

            while (i > n && node.ScriptTokenStream[i].TokenType != TSqlTokenType.LeftParenthesis)
            {
                i--;
            }

            // If there is anything between type name and the opening "("
            if (i - node.Name.LastTokenIndex > 1)
            {
                HandleTokenError(node.ScriptTokenStream[i - 1]);
            }
        }
    }
}
