using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0786", "LINE_NO")]
    internal sealed class LineNoRule : AbstractRule
    {
        public LineNoRule() : base()
        {
        }

        public override void Visit(LineNoStatement node) => HandleNodeError(node);
    }
}
