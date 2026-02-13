using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("SI0847", "MULTIPLE_IN_TO_SINGLE")]
    internal sealed class MultipleInPredicateIntoOneRule : AbstractRule
    {
        private readonly CollapsibleInExtractor extractor = new CollapsibleInExtractor(false);

        public MultipleInPredicateIntoOneRule() : base()
        {
        }

        // Explicit - to avoid double visiting parts of complex AND-OR-AND-OR expression.
        public override void ExplicitVisit(BooleanBinaryExpression node)
        {
            if (node.BinaryExpressionType != BooleanBinaryExpressionType.Or)
            {
                // Forwarding to base visitor so the nested binary expressions will be processed
                Visit(node);

                // Only OR is supported by the rule
                return;
            }

            extractor.Process(node, ViolationHandlerWithMessage);
        }
    }
}
