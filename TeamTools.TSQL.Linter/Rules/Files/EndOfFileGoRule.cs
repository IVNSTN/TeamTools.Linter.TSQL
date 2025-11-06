using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FL0308", "EOF_GO")]
    internal sealed class EndOfFileGoRule : AbstractRule
    {
        public EndOfFileGoRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            if (node.ScriptTokenStream == null)
            {
                return;
            }

            int i = node.ScriptTokenStream.Count - 1;

            while (i >= 0
                && (node.ScriptTokenStream[i].TokenType == TSqlTokenType.WhiteSpace
                || node.ScriptTokenStream[i].TokenType == TSqlTokenType.EndOfFile
                || node.ScriptTokenStream[i].TokenType == TSqlTokenType.Semicolon))
            {
                i--;
            }

            if (i < 0)
            {
                // no SQL tokens found
                return;
            }

            if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.Go)
            {
                // GO found
                return;
            }

            HandleLineError(node.ScriptTokenStream[i].Line, 1);
        }
    }
}
