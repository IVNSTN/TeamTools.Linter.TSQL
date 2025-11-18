using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

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
            if (node.Expression?.ExtractScalarExpression() is NullLiteral)
            {
                HandleNodeError(node.Expression);
            }
        }
    }
}
