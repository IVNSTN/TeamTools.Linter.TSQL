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
            foreach (var tbl in node.Objects)
            {
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
