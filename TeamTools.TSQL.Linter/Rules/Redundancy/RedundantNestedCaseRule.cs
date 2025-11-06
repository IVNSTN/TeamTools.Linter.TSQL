using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0287", "REDUNDANT_NESTED_CASE")]
    internal sealed class RedundantNestedCaseRule : AbstractRule
    {
        public RedundantNestedCaseRule() : base()
        {
        }

        public override void Visit(SimpleCaseExpression node)
        {
            if (node.ElseExpression is SimpleCaseExpression nestedCase
            && ExpressionsAreEqual(node.InputExpression, nestedCase.InputExpression))
            {
                HandleNodeError(nestedCase.InputExpression);
            }
        }

        public override void Visit(SearchedCaseExpression node)
        {
            if (node.ElseExpression is SearchedCaseExpression)
            {
                HandleNodeError(node.ElseExpression);
            }
        }

        private static bool ExpressionsAreEqual(ScalarExpression a, ScalarExpression b)
        {
            string textA = ExpandExpression(a).GetFragmentCleanedText();
            string textB = ExpandExpression(b).GetFragmentCleanedText();

            return string.Equals(textA, textB, StringComparison.OrdinalIgnoreCase);
        }

        // TODO : also remove redundant nested parenthesis from within the expression
        private static ScalarExpression ExpandExpression(ScalarExpression node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            return node;
        }
    }
}
