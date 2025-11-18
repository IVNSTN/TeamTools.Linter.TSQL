using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
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
            if (node.AsFileTable)
            {
                // Filetable has no columns
                return;
            }

            if (node.SchemaObjectName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                return;
            }

            int n = node.Definition.ColumnDefinitions.Count;
            for (int i = 0; i < n; i++)
            {
                HandleNodeErrorIfAny(DetectTableLevelConstraint(node.Definition.ColumnDefinitions[i].Constraints));
            }
        }

        private static ConstraintDefinition DetectTableLevelConstraint(IList<ConstraintDefinition> constraints)
        {
            int n = constraints.Count;
            for (int i = 0; i < n; i++)
            {
                var cst = constraints[i];

                if (cst is CheckConstraintDefinition
                || cst is ForeignKeyConstraintDefinition
                || cst is UniqueConstraintDefinition)
                {
                    return cst;
                }
            }

            return default;
        }
    }
}
