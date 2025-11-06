using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // very similar to UnionToUnionAllRule
    [RuleIdentity("CS0914", "INTERSECT_EXCEPT_BROKEN_BY_LITERAL")]
    internal sealed class IntersectExceptOnLiteralRule : AbstractRule
    {
        public IntersectExceptOnLiteralRule() : base()
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
