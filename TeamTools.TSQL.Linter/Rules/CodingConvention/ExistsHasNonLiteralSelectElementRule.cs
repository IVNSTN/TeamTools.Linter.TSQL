using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0173", "EXISTS_NONLITERAL_SELECT")]
    internal sealed class ExistsHasNonLiteralSelectElementRule : AbstractRule
    {
        public ExistsHasNonLiteralSelectElementRule() : base()
        {
        }

        public override void Visit(ExistsPredicate node)
        {
            if (node.Subquery.QueryExpression is BinaryQueryExpression bin
            && bin.BinaryQueryExpressionType != BinaryQueryExpressionType.Union)
            {
                // intersect/except need columns
                return;
            }

            var qs = node.Subquery.QueryExpression.GetQuerySpecification();
            if (qs is null)
            {
                return;
            }

            var nonLiteralElementDetector = new SelectElementIsNonLiteral();

            var badElement = qs.SelectElements
                .FirstOrDefault(col =>
                {
                    col.Accept(nonLiteralElementDetector);
                    return nonLiteralElementDetector.Detected;
                });

            HandleNodeErrorIfAny(badElement);
        }

        private class SelectElementIsNonLiteral : TSqlViolationDetector
        {
            public override void Visit(QueryDerivedTable node) => MarkDetected(node);

            public override void Visit(ColumnReferenceExpression node) => MarkDetected(node);
        }
    }
}
