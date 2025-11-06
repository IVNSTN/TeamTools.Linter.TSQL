using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0705", "INVALID_ARGUMENT")]
    internal sealed class InvalidFunctionArgumentRule : AbstractRule
    {
        public InvalidFunctionArgumentRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var expressionEvaluator = new ScalarExpressionEvaluator(node);

            var violations = expressionEvaluator
                .Violations
                .Where(v => v is InvalidArgumentViolation || v is ArgumentOutOfRangeViolation)
                .Where(v => v.Source?.Node != null)
                .ToList();

            foreach (var v in violations)
            {
                HandleNodeError(v.Source.Node, v.Message);
            }
        }
    }
}
