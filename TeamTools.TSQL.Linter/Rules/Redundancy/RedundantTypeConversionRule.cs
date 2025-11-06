using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0706", "REDUNDANT_TYPE_CONVERSION")]
    internal sealed class RedundantTypeConversionRule : AbstractRule
    {
        public RedundantTypeConversionRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var expressionEvaluator = new ScalarExpressionEvaluator(node);

            var violations = expressionEvaluator
                .Violations
                .OfType<RedundantTypeConversionViolation>()
                .Where(v => v.Source?.Node != null)
                .ToList();

            foreach (var v in violations)
            {
                HandleNodeError(v.Source.Node, v.Message);
            }
        }
    }
}
