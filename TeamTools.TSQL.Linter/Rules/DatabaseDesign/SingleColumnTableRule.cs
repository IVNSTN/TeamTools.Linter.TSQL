using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0825", "SINGLE_COL_TABLE")]
    internal sealed class SingleColumnTableRule : AbstractRule
    {
        public SingleColumnTableRule() : base()
        {
        }

        // FIXME : ignore # and @
        public override void Visit(TableDefinition node)
        {
            if (node?.ColumnDefinitions is null)
            {
                // e.g. filetable
                return;
            }

            if (node.ColumnDefinitions.Count == 1)
            {
                HandleNodeError(node.ColumnDefinitions[0]);
            }
        }
    }
}
