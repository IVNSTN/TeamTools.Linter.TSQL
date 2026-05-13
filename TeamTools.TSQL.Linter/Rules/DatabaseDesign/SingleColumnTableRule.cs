using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0825", "SINGLE_COL_TABLE")]
    internal sealed class SingleColumnTableRule : AbstractRule
    {
        public SingleColumnTableRule() : base()
        {
        }

        public override void ExplicitVisit(CreateTableStatement node)
        {
            if (node.SchemaObjectName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                // Temp tables should be ignored
                return;
            }

            ValidateDefinition(node.Definition);
        }

        private void ValidateDefinition(TableDefinition node)
        {
            if (node?.ColumnDefinitions is null)
            {
                // e.g. filetable
                return;
            }

            if (node.ColumnDefinitions.Count == 1)
            {
                HandleNodeError(node.ColumnDefinitions[0].ColumnIdentifier);
            }
        }
    }
}
