using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0139", "TRIGGER_ORDERING")]
    [TriggerRule]
    internal sealed class TriggerOrderRule : AbstractRule
    {
        public TriggerOrderRule() : base()
        {
        }

        public override void Visit(ExecutableProcedureReference node)
        {
            if (node.ProcedureReference.ProcedureVariable != null)
            {
                return;
            }

            if (node.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value.ToLower() == "sp_settriggerorder")
            {
                HandleNodeError(node);
            }
        }
    }
}
