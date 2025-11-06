using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0716", "IF_ELSE_REDUNDANT_BEGIN_END")]
    internal sealed class RedundantBeginEndForElseRule : AbstractRule
    {
        public RedundantBeginEndForElseRule() : base()
        {
        }

        public override void Visit(IfStatement node)
        {
            if (node.ElseStatement is null || !(node.ElseStatement is BeginEndBlockStatement beginEnd))
            {
                return;
            }

            if (beginEnd.StatementList.Statements.Count == 1
            && beginEnd.StatementList.Statements[0] is IfStatement)
            {
                HandleNodeError(beginEnd);
            }
        }
    }
}
