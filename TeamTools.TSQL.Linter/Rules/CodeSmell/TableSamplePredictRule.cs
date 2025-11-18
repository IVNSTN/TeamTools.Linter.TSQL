using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0789", "TABLESAMPLE_PREDICT")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    internal sealed class TableSamplePredictRule : AbstractRule
    {
        public TableSamplePredictRule() : base()
        {
        }

        public override void Visit(TableSampleClause node) => HandleNodeError(node);

        public override void Visit(PredictTableReference node) => HandleNodeError(node);
    }
}
