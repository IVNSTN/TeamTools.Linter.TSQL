using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0803", "SYSTYPE_HAS_SCHEMA")]
    internal sealed class SystemTypeWithSchemaRule : AbstractRule
    {
        public SystemTypeWithSchemaRule() : base()
        {
        }

        public override void Visit(SqlDataTypeReference node)
        {
            if (node.Name is null)
            {
                // e.g. CURSOR
                return;
            }

            HandleNodeErrorIfAny(node.Name.SchemaIdentifier, node.Name.BaseIdentifier.Value);
        }
    }
}
