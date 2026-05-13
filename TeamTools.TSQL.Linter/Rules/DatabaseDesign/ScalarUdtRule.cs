using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0855", "SCALAR_UDT")]
    internal sealed class ScalarUdtRule : AbstractRule
    {
        public ScalarUdtRule() : base()
        {
        }

        public override void ExplicitVisit(CreateTypeUddtStatement node) => HandleNodeError(node.DataType, node.Name.GetFullName());
    }
}
