using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0293", "REDUNDANT_CASE_ELSE_NULL")]
    internal sealed class RedundantCaseElseNullRule : AbstractRule
    {
        public RedundantCaseElseNullRule() : base()
        {
        }

        public override void Visit(CaseExpression node)
        {
            if (node.ElseExpression?.ExtractScalarExpression() is NullLiteral)
            {
                HandleNodeError(node.ElseExpression);
            }
        }
    }
}
