using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0739", "SCHEMA_NEEDS_NAME")]
    internal sealed class SchemaNeedsNameRule : AbstractRule
    {
        public SchemaNeedsNameRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE SCHEMA must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is CreateSchemaStatement sc)
            {
                DoValidate(sc);
            }
        }

        private void DoValidate(CreateSchemaStatement node)
        {
            if (node.Name is null)
            {
                HandleNodeError(node);
            }
        }
    }
}
