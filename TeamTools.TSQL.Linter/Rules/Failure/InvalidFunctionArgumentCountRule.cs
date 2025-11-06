using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0704", "INVALID_ARGUMENT_COUNT")]
    internal sealed class InvalidFunctionArgumentCountRule : AbstractRule
    {
        public InvalidFunctionArgumentCountRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var expressionEvaluator = new ScalarExpressionEvaluator(node);

            var violations = expressionEvaluator
                .Violations
                .OfType<InvalidNumberOfArgumentsViolation>()
                .Where(v => v.Source?.Node != null)
                .ToList();

            foreach (var v in violations)
            {
                HandleNodeError(v.Source.Node, v.Message);
            }
        }
    }
}
