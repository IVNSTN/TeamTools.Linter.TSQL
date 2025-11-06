using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0274", "NUMBERED_SP")]
    internal sealed class NumberedSpRule : AbstractRule
    {
        public NumberedSpRule() : base()
        {
        }

        public override void Visit(ProcedureStatementBody node) => HandleNodeErrorIfAny(node.ProcedureReference.Number);
    }
}
