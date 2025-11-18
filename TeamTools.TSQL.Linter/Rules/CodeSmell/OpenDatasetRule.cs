using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0790", "UNEXPECTED_OPENDATASET")]
    internal sealed class OpenDatasetRule : AbstractRule
    {
        public OpenDatasetRule() : base()
        {
        }

        public override void Visit(OpenRowsetTableReference node) => HandleNodeError(node);

        public override void Visit(OpenRowsetCosmos node) => HandleNodeError(node);

        public override void Visit(BulkOpenRowset node) => HandleNodeError(node);

        public override void Visit(AdHocTableReference node) => HandleNodeErrorIfAny(node.DataSource);
    }
}
