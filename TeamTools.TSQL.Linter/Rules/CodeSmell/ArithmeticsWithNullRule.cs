using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0710", "NULL_ARITHMETICS")]
    internal sealed class ArithmeticsWithNullRule : AbstractRule
    {
        public ArithmeticsWithNullRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var expressionEvaluator = new ScalarExpressionEvaluator(node);

            var violations = expressionEvaluator
                .Violations
                .OfType<NullArithmeticsViolation>()
                .Where(v => v.Source?.Node != null)
                .ToList();

            foreach (var v in violations)
            {
                HandleNodeError(v.Source.Node, v.Message);
            }
        }
    }
}
