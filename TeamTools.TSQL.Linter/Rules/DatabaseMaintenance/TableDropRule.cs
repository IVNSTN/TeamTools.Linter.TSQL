using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DM0515", "DROPPING_TABLES")]
    internal sealed class TableDropRule : AbstractRule
    {
        public TableDropRule() : base()
        {
        }

        public override void Visit(DropTableStatement node)
        {
            int n = node.Objects.Count;
            for (int i = 0; i < n; i++)
            {
                var tbl = node.Objects[i];

                if (!tbl.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
                {
                    HandleNodeError(tbl);
                    // one error per drop is fine
                    return;
                }
            }
        }
    }
}
