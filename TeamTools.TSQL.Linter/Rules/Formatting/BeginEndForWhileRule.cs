using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0150", "WHILE_BEGIN_END")]
    internal sealed class BeginEndForWhileRule : AbstractRule
    {
        public BeginEndForWhileRule() : base()
        {
        }

        public override void Visit(WhileStatement node)
        {
            if (node.Statement is BeginEndBlockStatement)
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
