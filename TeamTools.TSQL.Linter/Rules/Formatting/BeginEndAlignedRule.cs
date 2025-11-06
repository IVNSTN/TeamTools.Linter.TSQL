using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0218", "BEGIN_END_ALIGN")]
    internal sealed class BeginEndAlignedRule : AbstractRule
    {
        public BeginEndAlignedRule() : base()
        {
        }

        public override void Visit(BeginEndBlockStatement node)
        {
            int beginIndex = node.FirstTokenIndex;
            int endIndex = node.LastTokenIndex;

            while ((endIndex > beginIndex) && (node.ScriptTokenStream[endIndex].TokenType != TSqlTokenType.End))
            {
                endIndex--;
            }

            if (node.ScriptTokenStream[beginIndex].Line == node.ScriptTokenStream[endIndex].Line)
            {
                // ignoring one-line blocks
                return;
            }

            if (node.ScriptTokenStream[beginIndex].Column != node.ScriptTokenStream[endIndex].Column)
            {
                HandleNodeError(node);
            }
        }
    }
}
