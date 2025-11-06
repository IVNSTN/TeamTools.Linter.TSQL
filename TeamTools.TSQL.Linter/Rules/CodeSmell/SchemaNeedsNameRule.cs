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

        public override void Visit(CreateSchemaStatement node)
        {
            if (node.Name is null)
            {
                HandleNodeError(node);
            }
        }
    }
}
