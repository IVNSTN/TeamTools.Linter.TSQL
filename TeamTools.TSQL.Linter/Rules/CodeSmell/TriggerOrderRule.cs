using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0139", "TRIGGER_ORDERING")]
    [TriggerRule]
    internal sealed class TriggerOrderRule : AbstractRule
    {
        private static readonly string SetTriggerOrderProcName = "sp_settriggerorder";

        public TriggerOrderRule() : base()
        {
        }

        public override void Visit(ExecutableProcedureReference node)
        {
            if (node.ProcedureReference.ProcedureVariable != null)
            {
                return;
            }

            if (string.Equals(SetTriggerOrderProcName, node.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value, StringComparison.OrdinalIgnoreCase))
            {
                HandleNodeError(node);
            }
        }
    }
}
