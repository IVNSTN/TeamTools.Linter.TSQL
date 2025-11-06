using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0947", "LITERAL_OUT_OF_RANGE")]
    internal sealed class LiteralOutOfRangeRule : AbstractRule
    {
        public LiteralOutOfRangeRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var expressionEvaluator = new ScalarExpressionEvaluator(node);

            var violations = expressionEvaluator
                .Violations
                .OfType<OutOfRangeViolation>()
                .Where(v => v.ValueSource?.Node != null)
                .Where(v => v.ValueSource.SourceKind == SqlValueSourceKind.Literal)
                .ToList();

            foreach (var v in violations)
            {
                HandleNodeError(v.ValueSource.Node, v.Message);
            }
        }
    }
}
