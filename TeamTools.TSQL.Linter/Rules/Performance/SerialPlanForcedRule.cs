using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0872", "SERIAL_PLAN_FORCED")]
    internal sealed class SerialPlanForcedRule : AbstractRule
    {
        public SerialPlanForcedRule() : base()
        {
        }

        public override void Visit(OptimizerHint node)
        {
            if (node.HintKind == OptimizerHintKind.MaxDop
            && node is LiteralOptimizerHint maxdop
            && int.TryParse(maxdop.Value.Value, out int maxdopValue)
            && maxdopValue == 1)
            {
                // MAXDOP 1 means no parallelization
                HandleNodeError(node, "MAXDOP 1");
            }
        }

        public override void Visit(QualifiedJoin node)
        {
            if (node.JoinHint == JoinHint.Remote)
            {
                // REMOTE means no parallelization
                HandleNodeError(node, "REMOTE");
            }
        }

        public override void Visit(OutputClause node)
        {
            // DML OUTPUT to client means no parallelization
            HandleNodeError(node, "OUTPUT");
        }
    }
}
