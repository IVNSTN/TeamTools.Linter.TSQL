using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0827", "COMPUTED_COL_CONST")]
    internal sealed class ComputedColumnIsConstantRule : AbstractRule
    {
        public ComputedColumnIsConstantRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node)
        {
            if (node.ComputedColumnExpression is null)
            {
                // not a computed col
                return;
            }

            var value = node.ComputedColumnExpression.ExtractScalarExpression();

            if (value != null && value is Literal)
            {
                HandleNodeError(value, node.ColumnIdentifier.Value);
            }
        }
    }
}
