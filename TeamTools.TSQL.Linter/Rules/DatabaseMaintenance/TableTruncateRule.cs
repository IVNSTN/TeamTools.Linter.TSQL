using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DM0516", "TRUNCATING_TABLES")]
    internal sealed class TableTruncateRule : AbstractRule
    {
        public TableTruncateRule() : base()
        {
        }

        public override void Visit(TruncateTableStatement node)
        {
            if (node.TableName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                // ignoring temp tables
                return;
            }

            HandleNodeError(node);
        }
    }
}
