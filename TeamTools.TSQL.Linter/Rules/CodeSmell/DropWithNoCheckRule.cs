using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0977", "DROPPING_WITH_NO_CHECK")]
    internal sealed partial class DropWithNoCheckRule : AbstractRule
    {
        public DropWithNoCheckRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            var dropVisitor = new DropVisitor();
            node.AcceptChildren(dropVisitor);

            if (!dropVisitor.SuspiciousDropsFound)
            {
                return;
            }

            var preceidingConditionVisitor = new ConditionalDropVisitor(dropVisitor.SuspiciousDrops);
            node.AcceptChildren(preceidingConditionVisitor);

            for (int i = dropVisitor.SuspiciousDrops.Count - 1; i >= 0; i--)
            {
                HandleNodeError(dropVisitor.SuspiciousDrops[i]);
            }
        }
    }
}
