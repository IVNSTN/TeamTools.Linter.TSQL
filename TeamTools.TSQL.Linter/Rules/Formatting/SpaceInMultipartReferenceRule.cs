using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0809", "SPACE_IN_NAME_REF")]
    internal sealed class SpaceInMultipartReferenceRule : AbstractRule
    {
        public SpaceInMultipartReferenceRule() : base()
        {
        }

        public override void Visit(MultiPartIdentifier node)
        {
            if (node.FirstTokenIndex < 0)
            {
                // some parser bug
                return;
            }

            int n = node.LastTokenIndex + 1;
            for (var i = node.FirstTokenIndex; i < n; i++)
            {
                var token = node.ScriptTokenStream[i];
                if (ScriptDomExtension.IsSkippableTokens(token.TokenType))
                {
                    HandleTokenError(token);
                }
            }
        }
    }
}
