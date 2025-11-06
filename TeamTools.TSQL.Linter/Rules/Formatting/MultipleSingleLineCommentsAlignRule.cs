using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0246", "MANY_SL_COMMENTS_ALIGNED")]
    internal sealed class MultipleSingleLineCommentsAlignRule : AbstractRule
    {
        public MultipleSingleLineCommentsAlignRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            int lastCommentColumn = -1;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;

            for (int i = start; i <= end; i++)
            {
                if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.WhiteSpace)
                {
                    // do nothing
                }
                else if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.SingleLineComment)
                {
                    if ((lastCommentColumn >= 0) && (lastCommentColumn != node.ScriptTokenStream[i].Column))
                    {
                        HandleTokenError(node.ScriptTokenStream[i]);
                    }

                    lastCommentColumn = node.ScriptTokenStream[i].Column;
                }
                else
                {
                    lastCommentColumn = -1;
                }
            }
        }
    }
}
