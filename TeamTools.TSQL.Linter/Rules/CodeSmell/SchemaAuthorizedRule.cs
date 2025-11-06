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

        public override void Visit(CreateSchemaStatement node)
        {
            if (node.Owner != null)
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
