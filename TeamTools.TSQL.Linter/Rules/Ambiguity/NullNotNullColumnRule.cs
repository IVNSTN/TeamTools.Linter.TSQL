using Microsoft.SqlServer.TransactSql.ScriptDom;
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
            int n = node.ColumnDefinitions.Count;
            for (int i = 0; i < n; i++)
            {
                var col = node.ColumnDefinitions[i];
                if (col.ComputedColumnExpression != null)
                {
                    // Computed columns can't have NOT NULL constraint
                    continue;
                }

                int m = col.Constraints.Count;
                int nullConstraintCount = 0;
                TSqlFragment lastNullConstraint = default;
                for (int j = 0; j < m; j++)
                {
                    if (col.Constraints[j] is NullableConstraintDefinition)
                    {
                        nullConstraintCount++;
                        lastNullConstraint = col.Constraints[j];
                    }
                }

                if (nullConstraintCount == 0)
                {
                    HandleNodeError(col, col.ColumnIdentifier.Value);
                }
                else if (nullConstraintCount > 1)
                {
                    // somehow syntax allows declaring both NULL and NOT NULL - this must be detected too
                    HandleNodeError(lastNullConstraint, col.ColumnIdentifier.Value);
                }
            }
        }
    }
}
