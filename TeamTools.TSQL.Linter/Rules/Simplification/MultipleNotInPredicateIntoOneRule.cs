using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("SI0848", "MULTIPLE_NOT_IN_TO_SINGLE")]
    internal sealed class MultipleNotInPredicateIntoOneRule : AbstractRule
    {
        private readonly CollapsibleInExtractor extractor = new CollapsibleInExtractor(true);

        public MultipleNotInPredicateIntoOneRule() : base()
        {
        }

        // Explicit - to avoid double visiting parts of complex AND-OR-AND-OR expression.
        public override void ExplicitVisit(BooleanBinaryExpression node)
        {
            if (node.BinaryExpressionType != BooleanBinaryExpressionType.And)
            {
                // Forwarding to base visitor so the nested binary expressions will be processed
                Visit(node);

                // Only AND is supported by the rule
                return;
            }

            extractor.Process(node, ViolationHandlerWithMessage);
        }
    }
}
