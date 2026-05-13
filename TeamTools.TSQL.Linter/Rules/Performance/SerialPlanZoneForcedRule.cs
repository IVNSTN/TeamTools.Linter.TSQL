using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [CompatibilityLevel(SqlVersion.Sql100, SqlVersion.Sql130)]
    [RuleIdentity("PF0874", "SERIAL_PLAN_ZONE_FORCED")]
    internal sealed class SerialPlanZoneForcedRule : AbstractRule
    {
        public SerialPlanZoneForcedRule() : base()
        {
        }

        public override void Visit(QualifiedJoin node)
        {
            if (node.JoinHint == JoinHint.Loop)
            {
                HandleNodeError(node, "LOOP JOIN");
            }
        }

        public override void Visit(TopRowFilter node)
        {
            HandleNodeError(node, "TOP");
        }

        public override void Visit(OptimizerHint node)
        {
            if (node.HintKind == OptimizerHintKind.MaxRecursion)
            {
                HandleNodeError(node, "MAXRECURSION");
            }
        }

        public override void Visit(FunctionCall node)
        {
            if (node.OverClause?.OrderByClause is null)
            {
                // not a windowed function call
                return;
            }

            if (IsNonParallelFunction(node.FunctionName.Value))
            {
                HandleNodeError(node, "ROW_NUMBER/RANK");
            }
        }

        private static bool IsNonParallelFunction(string functionName)
        {
            return functionName.Equals("ROW_NUMBER", StringComparison.OrdinalIgnoreCase)
                || functionName.Equals("RANK", StringComparison.OrdinalIgnoreCase)
                || functionName.Equals("STRING_AGG", StringComparison.OrdinalIgnoreCase);
        }
    }
}
