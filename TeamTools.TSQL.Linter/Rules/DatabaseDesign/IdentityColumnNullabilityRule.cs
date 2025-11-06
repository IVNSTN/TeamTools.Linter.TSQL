using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0183", "IDENTITY_NOT_NULL")]
    internal sealed class IdentityColumnNullabilityRule : AbstractRule
    {
        public IdentityColumnNullabilityRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node)
        {
            if (node.IdentityOptions == null)
            {
                return;
            }

            var csnull = node.Constraints.OfType<NullableConstraintDefinition>().FirstOrDefault();

            if (csnull != null && !csnull.Nullable)
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
