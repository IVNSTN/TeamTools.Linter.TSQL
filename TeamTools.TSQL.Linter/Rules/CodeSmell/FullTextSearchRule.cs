using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0751", "FULLTEXT_SEARCH")]
    internal sealed class FullTextSearchRule : AbstractRule
    {
        public FullTextSearchRule() : base()
        {
        }

        public override void Visit(FullTextTableReference node) => HandleNodeError(node);

        public override void Visit(FullTextPredicate node) => HandleNodeError(node);

        public override void Visit(FullTextCatalogStatement node) => HandleNodeError(node);
    }
}
