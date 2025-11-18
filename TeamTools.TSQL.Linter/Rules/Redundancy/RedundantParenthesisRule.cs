using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0247", "REDUNDANT_PARENTHESIS")]
    internal sealed class RedundantParenthesisRule : AbstractRule
    {
        public RedundantParenthesisRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node) => node.Accept(new ParenthesisValidator(ViolationHandler));

        private sealed class ParenthesisValidator : VisitorWithCallback
        {
            private readonly List<TSqlFragment> ignoredNodes = new List<TSqlFragment>();

            public ParenthesisValidator(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(ParenthesisExpression node)
            {
                if (ignoredNodes.Contains(node))
                {
                    return;
                }

                if (!IsValidExpressionForParenthesis(node.Expression))
                {
                    Callback(node);
                }
            }

            public override void Visit(TopRowFilter node) => ignoredNodes.Add(node.Expression);

            private static bool IsValidExpressionForParenthesis(ScalarExpression node)
            {
                while (node is UnaryExpression ue)
                {
                    node = ue.Expression;
                }

                if (!(node is PrimaryExpression))
                {
                    return true;
                }

                // PrimaryExpression includes Literal, VariableReference, ColumnReference, FunctionCall
                // and other simple expressions. But complex expressions should be allowed in parenhesis.
                return node is CaseExpression
                    || node is ScalarSubquery;
            }
        }
    }
}
