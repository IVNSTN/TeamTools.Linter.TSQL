using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0106", "COLUMN_NULLABLE")]
    internal sealed class NullNotNullColumnRule : AbstractRule
    {
        public NullNotNullColumnRule() : base()
        {
        }

        public override void Visit(TableDefinition node)
        {
            var badColDefinitions = node.ColumnDefinitions
                // Computed columns can't have NOT NULL constraint
                .Where(col => col.ComputedColumnExpression is null)
                // somehow syntax allows declaring both NULL and NOT NULL - this must be detected too
                .Where(col => col.Constraints.OfType<NullableConstraintDefinition>().Count() != 1);

            foreach (var col in badColDefinitions)
            {
                HandleNodeError(col, col.ColumnIdentifier.Value);
            }
        }
    }
}
