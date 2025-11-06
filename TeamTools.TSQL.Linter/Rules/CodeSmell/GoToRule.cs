using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0105", "GOTO")]
    internal sealed class GoToRule : AbstractRule
    {
        public GoToRule() : base()
        {
        }

        public override void Visit(GoToStatement node) => HandleNodeError(node);

        public override void Visit(LabelStatement node) => HandleNodeError(node);
    }
}
