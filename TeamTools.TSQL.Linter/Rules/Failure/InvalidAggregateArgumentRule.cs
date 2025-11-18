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

            if (!(fn.OverClause is null))
            {
                return;
            }

            if (IsSubquery(firstParam))
            {
                HandleNodeError(firstParam);
                return;
            }
        }

        private static bool IsNullLiteral(ScalarExpression node)
        {
            if (node is Literal lit)
            {
                return lit.LiteralType == LiteralType.Null;
            }

            if (node is ParenthesisExpression pe)
            {
                return IsNullLiteral(pe.Expression);
            }

            return false;
        }

        private static bool IsSubquery(ScalarExpression node)
        {
            if (node is ScalarSubquery)
            {
                return true;
            }

            if (node is ParenthesisExpression pe)
            {
                return IsNullLiteral(pe.Expression);
            }

            return false;
        }
    }
}
