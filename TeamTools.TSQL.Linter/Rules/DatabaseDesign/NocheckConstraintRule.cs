using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0913", "NOCHECK_CONSTRAINT")]
    internal sealed class NocheckConstraintRule : AbstractRule
    {
        public NocheckConstraintRule() : base()
        {
        }

        public override void Visit(AlterTableConstraintModificationStatement node)
        {
            if (node.ConstraintEnforcement != ConstraintEnforcement.NoCheck)
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
