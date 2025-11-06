using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DM0752", "FULLTEXT_CATALOG_MANAGEMENT")]
    internal sealed class FullTextCatalogManagementRule : AbstractRule
    {
        public FullTextCatalogManagementRule() : base()
        {
        }

        public override void Visit(CreateFullTextCatalogStatement node) => HandleNodeError(node);

        public override void Visit(CreateFullTextStopListStatement node) => HandleNodeError(node);

        public override void Visit(CreateFullTextIndexStatement node) => HandleNodeError(node);
    }
}
