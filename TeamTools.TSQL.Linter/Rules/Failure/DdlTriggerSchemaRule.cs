using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0711", "DDL_TRIGGER_NO_SCHEMA")]
    [TriggerRule]
    internal sealed class DdlTriggerSchemaRule : AbstractRule
    {
        public DdlTriggerSchemaRule() : base()
        {
        }

        public override void Visit(CreateTriggerStatement node)
        {
            if (node.TriggerObject.TriggerScope == TriggerScope.Normal)
            {
                // DML-triggers can have schema
                return;
            }

            HandleNodeErrorIfAny(node.Name.SchemaIdentifier);
        }
    }
}
