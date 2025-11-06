using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0266", "TABLE_LEVEL_CONSTRAINT_IN_COL")]
    internal sealed class TableLevelConstraintInColumnRule : AbstractRule
    {
        public TableLevelConstraintInColumnRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            if (node.SchemaObjectName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                return;
            }

            if ((node.Definition?.ColumnDefinitions.Count ?? 0) == 0)
            {
                // FILETABLE
                return;
            }

            foreach (var col in node.Definition.ColumnDefinitions)
            {
                if (col.Constraints.Where(cst =>
                    cst is CheckConstraintDefinition
                    || cst is ForeignKeyConstraintDefinition
                    || cst is UniqueConstraintDefinition).Any())
                {
                    HandleNodeError(col);
                }
            }
        }
    }
}
