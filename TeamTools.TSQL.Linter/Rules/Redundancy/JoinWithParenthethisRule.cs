using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0780", "JOIN_PREDICATE_PARENTHETHIS")]
    internal sealed class JoinWithParenthethisRule : AbstractRule
    {
        public JoinWithParenthethisRule() : base()
        {
        }

        public override void Visit(QualifiedJoin node)
        {
            if (node.SearchCondition is BooleanParenthesisExpression)
            {
                HandleNodeError(node.SearchCondition);
            }
        }
    }
}
