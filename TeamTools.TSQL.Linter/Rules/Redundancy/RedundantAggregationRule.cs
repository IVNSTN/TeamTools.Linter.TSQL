using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0943", "REDUNDANT_AGGREGATE")]
    internal sealed class RedundantAggregationRule : AbstractRule
    {
        // function name / specific redundant value or null if any literal
        private static readonly IDictionary<string, IgnoredValue> AggregateFunctions = new Dictionary<string, IgnoredValue>(StringComparer.OrdinalIgnoreCase);

        static RedundantAggregationRule()
        {
            // TODO : consolidate all the metadata about known built-in functions
            AggregateFunctions.Add("COUNT", IgnoredValue.Null);
            AggregateFunctions.Add("MAX", IgnoredValue.AnyLiteral);
            AggregateFunctions.Add("MIN", IgnoredValue.AnyLiteral);
            AggregateFunctions.Add("AVG", IgnoredValue.AnyLiteral);
            AggregateFunctions.Add("SUM", IgnoredValue.ZeroOrNull);
        }

        public RedundantAggregationRule() : base()
        {
        }

        [Flags]
        private enum IgnoredValue
        {
            AnyLiteral = 1,
            Null = 2,
            Zero = 4,
            ZeroOrNull = Null | Zero,
        }

        // TODO : AVG/MAX/MIN are redundant if column is in GROUP BY list
        public override void Visit(FunctionCall node)
        {
            if (!AggregateFunctions.ContainsKey(node.FunctionName.Value))
            {
                return;
            }

            if (node.Parameters.Count != 1)
            {
                return;
            }

            if (!IsBadLiteralParam(node.Parameters[0], AggregateFunctions[node.FunctionName.Value]))
            {
                return;
            }

            HandleNodeError(node);
        }

        private static bool IsBadLiteralParam(ScalarExpression param, IgnoredValue badValue)
        {
            while (param is ParenthesisExpression pe)
            {
                param = pe.Expression;
            }

            if (!(param is Literal lit))
            {
                return false;
            }

            if (badValue.HasFlag(IgnoredValue.AnyLiteral))
            {
                return true;
            }

            if (badValue.HasFlag(IgnoredValue.Zero) && lit.LiteralType == LiteralType.Integer
            && int.TryParse(lit.Value, out int paramValue)
            && paramValue == 0)
            {
                return true;
            }

            if (badValue.HasFlag(IgnoredValue.Null) && lit.LiteralType == LiteralType.Null)
            {
                return true;
            }

            return false;
        }
    }
}
