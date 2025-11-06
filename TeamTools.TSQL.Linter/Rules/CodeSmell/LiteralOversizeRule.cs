using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0946", "LITERAL_OVERSIZE")]
    internal sealed class LiteralOversizeRule : AbstractRule
    {
        public LiteralOversizeRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var expressionEvaluator = new ScalarExpressionEvaluator(node);

            var violations = expressionEvaluator
                .Violations
                .OfType<ImplicitTruncationViolation>()
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
