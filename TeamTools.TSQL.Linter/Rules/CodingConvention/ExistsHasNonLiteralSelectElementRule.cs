using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
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
            if (qs is null || qs.SelectElements.Count == 0)
            {
                return;
            }

            HandleNodeErrorIfAny(DetectFirst(qs.SelectElements));
        }

        private TSqlFragment DetectFirst(IList<SelectElement> sel)
        {
            var nonLiteralElementDetector = new SelectElementIsNonLiteral();

            int n = sel.Count;
            for (int i = 0; i < n; i++)
            {
                sel[i].Accept(nonLiteralElementDetector);
                if (nonLiteralElementDetector.Detected)
                {
                    return nonLiteralElementDetector.FirstDetectedNode;
                }
            }

            return default;
        }

        private sealed class SelectElementIsNonLiteral : TSqlViolationDetector
        {
            public override void Visit(QueryDerivedTable node) => MarkDetected(node);

            public override void Visit(ColumnReferenceExpression node) => MarkDetected(node);
        }
    }
}
