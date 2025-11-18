using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0805", "PRINTING")]
    internal sealed class PrintRule : AbstractRule
    {
        public PrintRule() : base()
        {
        }

        public override void Visit(PrintStatement node) => HandleNodeError(node);
    }
}
