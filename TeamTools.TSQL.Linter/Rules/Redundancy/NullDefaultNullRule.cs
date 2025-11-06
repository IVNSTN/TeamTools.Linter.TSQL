using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0185", "DEFAULT_NULL")]
    internal sealed class NullDefaultNullRule : AbstractRule
    {
        public NullDefaultNullRule() : base()
        {
        }

        public override void Visit(DefaultConstraintDefinition node)
        {
            if (IsNullLiteral(node.Expression))
            {
                HandleNodeError(node.Expression);
            }
        }

        private static bool IsNullLiteral(ScalarExpression node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            return node is NullLiteral;
        }
    }
}
