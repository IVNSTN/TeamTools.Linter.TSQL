using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0756", "DEADLOCK_PRIORITY_PRECISE")]
    internal sealed class DeadlockPriorityPreciseRule : AbstractRule
    {
        public DeadlockPriorityPreciseRule() : base()
        {
        }

        public override void Visit(GeneralSetCommand node)
        {
            if (node.CommandType != GeneralSetCommandType.DeadlockPriority)
            {
                return;
            }

            if (node.Parameter is VariableReference || node.Parameter is IntegerLiteral)
            {
                HandleNodeError(node);
            }
        }
    }
}
