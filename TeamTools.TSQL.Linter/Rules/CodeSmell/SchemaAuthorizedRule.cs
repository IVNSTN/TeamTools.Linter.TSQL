using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0118", "AUTHORIZED_SCHEMA")]
    internal sealed class SchemaAuthorizedRule : AbstractRule
    {
        public SchemaAuthorizedRule() : base()
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
            if (node.Owner != null)
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
