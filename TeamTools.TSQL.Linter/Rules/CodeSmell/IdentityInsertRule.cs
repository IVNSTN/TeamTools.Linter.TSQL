using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0748", "IDENTITY_INSERT")]
    internal sealed class IdentityInsertRule : AbstractRule
    {
        public IdentityInsertRule() : base()
        {
        }

        public override void Visit(SetIdentityInsertStatement node) => HandleNodeError(node);
    }
}
