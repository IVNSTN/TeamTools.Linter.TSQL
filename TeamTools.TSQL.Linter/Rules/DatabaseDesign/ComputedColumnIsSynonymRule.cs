using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0828", "COMPUTED_COL_SYNONYM")]
    internal sealed class ComputedColumnIsSynonymRule : AbstractRule
    {
        public ComputedColumnIsSynonymRule() : base()
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

            if (value != null && value is ColumnReferenceExpression colRef)
            {
                HandleNodeError(value, $"{node.ColumnIdentifier.Value} -> {colRef.MultiPartIdentifier.GetLastIdentifier().Value}");
            }
        }
    }
}
