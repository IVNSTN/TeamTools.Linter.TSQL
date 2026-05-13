using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0891", "SELECT_NULL_TYPE")]
    internal sealed class SelectNullTypeRule : AbstractRule
    {
        public SelectNullTypeRule() : base()
        {
        }

        public override void Visit(OutputClause node) => ValidateSelectedValues(node.SelectColumns);

        // No exception for SELECT-INTO because resulting table is affected by the ambiguity too.
        // Not visiting QueryExpression abstraction because it catches too much.
        // Also ignoring QueryDerivedTable visiting which may produce many false-positive detections.
        public override void Visit(SelectStatement node)
        {
            if (node.QueryExpression.ForClause != null)
            {
                // FOR XML / JSON don't really need type for NULL
                return;
            }

            ValidateCte(node.WithCtesAndXmlNamespaces?.CommonTableExpressions);
            ValidateQuery(node.QueryExpression);
        }

        private void ValidateCte(IList<CommonTableExpression> ctes)
        {
            if (ctes is null)
            {
                return;
            }

            for (int i = ctes.Count - 1; i >= 0; i--)
            {
                ValidateQuery(ctes[i].QueryExpression);
            }
        }

        private void ValidateQuery(QueryExpression q)
        {
            if (q.ForClause != null)
            {
                // FOR XML / JSON don't really need type for NULL
                return;
            }

            ValidateSelectedValues(q.GetQuerySpecification().SelectElements);
        }

        private void ValidateSelectedValues(IList<SelectElement> selectedElements)
        {
            for (int i = selectedElements.Count - 1; i >= 0; i--)
            {
                if (selectedElements[i] is SelectScalarExpression exp
                && exp.Expression.ExtractScalarExpression() is NullLiteral selectNull)
                {
                    HandleNodeError(selectNull);
                }
            }
        }
    }
}
