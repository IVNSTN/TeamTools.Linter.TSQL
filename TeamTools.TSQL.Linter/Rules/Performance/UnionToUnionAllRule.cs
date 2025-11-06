using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0238", "UNION_TO_UNION_ALL")]
    // very similar to IntersectExceptOnLiteralRule
    internal sealed class UnionToUnionAllRule : AbstractRule
    {
        public UnionToUnionAllRule() : base()
        {
        }

        public override void Visit(BinaryQueryExpression node)
        {
            if (node.BinaryQueryExpressionType != BinaryQueryExpressionType.Union)
            {
                return;
            }

            // already ALL defined
            if (node.All)
            {
                return;
            }

            var literal = BinaryQueryConflictingLiteralExtractor.GetFirstDifferentLiteral(node);
            HandleNodeErrorIfAny(literal);
        }
    }
}
