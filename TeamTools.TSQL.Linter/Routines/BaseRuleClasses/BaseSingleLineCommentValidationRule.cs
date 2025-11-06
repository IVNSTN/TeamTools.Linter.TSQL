using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.Linter.Routines
{
    internal abstract class BaseSingleLineCommentValidationRule : AbstractRule
    {
        public BaseSingleLineCommentValidationRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var badComments = node.ScriptTokenStream
                .Where(t => t.TokenType == TSqlTokenType.SingleLineComment)
                .Where(c => !IsValidCommentFormat(c.Text));

            foreach (var comment in badComments)
            {
                HandleLineError(comment.Line, comment.Column);
            }
        }

        protected abstract bool IsValidCommentFormat(string text);
    }
}
