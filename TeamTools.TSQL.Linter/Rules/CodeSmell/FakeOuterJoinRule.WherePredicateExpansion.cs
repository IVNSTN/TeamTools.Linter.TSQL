using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class FakeOuterJoinRule
    {
        private static IEnumerable<BooleanExpression> ExpandPredicate(BooleanExpression predicate)
        {
            while (predicate is BooleanParenthesisExpression pe)
            {
                predicate = pe.Expression;
            }

            // OR predicate referencing OUTER JOIN source may be absolutely valid
            if (predicate is BooleanBinaryExpression bin && bin.BinaryExpressionType == BooleanBinaryExpressionType.And)
            {
                foreach (var e in ExpandPredicate(bin.FirstExpression))
                {
                    yield return e;
                }

                foreach (var e in ExpandPredicate(bin.SecondExpression))
                {
                    yield return e;
                }
            }
            else
            {
                yield return predicate;
            }
        }

        private static ScalarExpression ExpandExpression(ScalarExpression expr)
        {
            while (expr is ParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            if (expr is UnaryExpression un)
            {
                // sign has no meaning in our case
                return ExpandExpression(un.Expression);
            }

            return expr;
        }

        private static string GetReferencedSourceFullName(IList<Identifier> id)
        {
            if (id.Count == 2)
            {
                // 'src_alias.col' => 'src_alias'
                return id[0].Value;
            }

            var sb = ObjectPools.StringBuilderPool.Get();

            // All name parts except the last one (column name) are needed
            for (int i = 0, n = id.Count - 1; i < n; i++)
            {
                if (i > 0)
                {
                    sb.Append(TSqlDomainAttributes.NamePartSeparator);
                }

                sb.Append(id[i].Value);
            }

            var fullName = sb.ToString();
            ObjectPools.StringBuilderPool.Return(sb);
            return fullName;
        }
    }
}
