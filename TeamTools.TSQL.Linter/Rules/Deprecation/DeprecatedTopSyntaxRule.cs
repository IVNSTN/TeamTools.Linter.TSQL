using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0407", "DEPRECATED_TOP_SYNTAX")]
    internal sealed class DeprecatedTopSyntaxRule : AbstractRule
    {
        public DeprecatedTopSyntaxRule() : base()
        {
        }

        public override void Visit(TopRowFilter node)
        {
            if (node.Expression is ParenthesisExpression)
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
