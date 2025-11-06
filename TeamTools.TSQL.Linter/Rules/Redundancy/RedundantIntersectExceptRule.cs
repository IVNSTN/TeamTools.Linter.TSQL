using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0784", "REDUNDANT_INTERSECT_EXCEPT")]
    // Very similar to UnionToUnionAllRule
    internal sealed class RedundantIntersectExceptRule : AbstractRule
    {
        public RedundantIntersectExceptRule() : base()
        {
        }

        public override void Visit(BinaryQueryExpression node)
        {
            if (node.BinaryQueryExpressionType == BinaryQueryExpressionType.Union)
            {
                return;
            }

            var literal = BinaryQueryConflictingLiteralExtractor.GetFirstDifferentLiteral(node);
            HandleNodeErrorIfAny(literal);
        }
    }
}
