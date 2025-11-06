using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0736", "DOUBLE_QUOTES")]
    internal sealed class DoubleQuotesRule : AbstractRule
    {
        public DoubleQuotesRule() : base()
        {
        }

        public override void Visit(Identifier node)
        {
            if (node.QuoteType == QuoteType.DoubleQuote)
            {
                HandleNodeError(node);
            }
        }

        public override void Visit(StringLiteral node)
        {
            if (!string.IsNullOrEmpty(node.Value)
            && node.ScriptTokenStream[node.FirstTokenIndex].Text.StartsWith("\""))
            {
                HandleNodeError(node);
            }
        }
    }
}
