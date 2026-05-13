using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0885", "RAISERROR_NEEDS_WITH_LOG")]
    internal sealed class RaiseErrorNeedsWithLogRule : AbstractRule
    {
        private static readonly int MinSeverityToNeedWithLog = 19;

        public RaiseErrorNeedsWithLogRule() : base()
        {
        }

        public override void Visit(RaiseErrorStatement node)
        {
            if ((node.RaiseErrorOptions & RaiseErrorOptions.Log) != 0)
            {
                // WITH LOG defined
                return;
            }

            if (node.SecondParameter is Literal l && int.TryParse(l.Value, out int severityValue)
            && severityValue >= MinSeverityToNeedWithLog)
            {
                HandleNodeError(node, l.Value);
            }
        }
    }
}
