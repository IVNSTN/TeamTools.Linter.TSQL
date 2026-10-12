using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0939", "INVALID_AGGREGATE_ARG")]
    internal sealed class InvalidAggregateArgumentRule : AbstractRule
    {
        // TODO : consolidate all the metadata about known built-in functions
        private static readonly HashSet<string> AggregateFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "COUNT",
            "SUM",
            "AVG",
            "MIN",
            "MAX",
        };

        public InvalidAggregateArgumentRule() : base()
        {
        }

        public override void Visit(SelectScalarExpression node)
        {
            if (!(node.Expression is FunctionCall fn))
            {
                return;
            }

            if (!AggregateFunctions.Contains(fn.FunctionName.Value))
            {
                return;
            }

            if (fn.Parameters.Count != 1)
            {
                return;
            }

            var firstParam = fn.Parameters[0];

            if (IsNullLiteral(firstParam))
            {
                HandleNodeError(firstParam);
                return;
            }

            if (fn.OverClause != null)
            {
                return;
            }

            if (IsSubquery(firstParam))
            {
                HandleNodeError(firstParam);
            }
        }

        private static bool IsNullLiteral(ScalarExpression node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            return node is Literal lit && lit.LiteralType == LiteralType.Null;
        }

        private static bool IsSubquery(ScalarExpression node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            return node is ScalarSubquery;
        }
    }
}
