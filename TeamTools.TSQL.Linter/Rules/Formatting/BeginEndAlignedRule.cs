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

            var begin = node.ScriptTokenStream[beginIndex];
            var end = node.ScriptTokenStream[endIndex];

            // in case of trailing whitespaces, comment or semicolon
            while (end.TokenType != TSqlTokenType.End)
            {
                end = node.ScriptTokenStream[--endIndex];
            }

            if (begin.Line == end.Line)
            {
                // ignoring one-line blocks
                return;
            }

            if (begin.Column != end.Column)
            {
                HandleNodeError(node);
            }
        }
    }
}
