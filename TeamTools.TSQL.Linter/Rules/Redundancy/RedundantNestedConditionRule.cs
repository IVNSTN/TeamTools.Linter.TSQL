using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0782", "REDUNDANT_NESTED_CONDITION")]
    internal sealed class RedundantNestedConditionRule : AbstractRule
    {
        public RedundantNestedConditionRule() : base()
        {
        }

        // TODO : maybe WHILE too?
        public override void Visit(IfStatement node)
        {
            // FIXME : still some false-positive violations can be reported
            if (node.ThenStatement is IfStatement
            || (node.ThenStatement is BeginEndBlockStatement be && be.StatementList.Statements.Count > 0
            && be.StatementList.Statements[0] is IfStatement))
            {
                node.ThenStatement.Accept(new DiveDeepPredicate(node.Predicate, HandleNodeError));
            }
        }

        // TODO : support SimpleWhenClause
        public override void Visit(SearchedWhenClause node) => node.ThenExpression.Accept(new DiveDeepPredicate(node.WhenExpression, HandleNodeError));

        private class DiveDeepPredicate : TSqlFragmentVisitor
        {
            private readonly ICollection<KeyValuePair<string, TSqlFragment>> appliedPredicates = new List<KeyValuePair<string, TSqlFragment>>();

            public DiveDeepPredicate(BooleanExpression predicate, Action<TSqlFragment, string> callback)
            {
                Callback = callback;

                foreach (var p in ExplodePredicate(predicate))
                {
                    var dup = appliedPredicates.FirstOrDefault(a => string.Equals(a.Key, p.Key, StringComparison.OrdinalIgnoreCase)).Value;

                    if (dup != null)
                    {
                        ReportOn(p.Value, dup.StartLine);
                    }
                    else
                    {
                        appliedPredicates.Add(p);
                    }
                }
            }

            public Action<TSqlFragment, string> Callback { get; }

            public override void Visit(IfStatement node) => DetectMatches(node.Predicate);

            public override void Visit(SearchedWhenClause node) => DetectMatches(node.WhenExpression);

            public override void Visit(IIfCall node) => DetectMatches(node.Predicate);

            private static IEnumerable<KeyValuePair<string, TSqlFragment>> ExplodePredicate(BooleanExpression predicate)
            {
                while (predicate is BooleanParenthesisExpression pe)
                {
                    predicate = pe.Expression;
                }

                // ORs are hard to understand from code
                if (predicate is BooleanBinaryExpression bin)
                {
                    if (bin.BinaryExpressionType == BooleanBinaryExpressionType.And)
                    {
                        return ExplodePredicate(bin.FirstExpression)
                            .Union(ExplodePredicate(bin.SecondExpression));
                    }
                    else
                    {
                        return Enumerable.Empty<KeyValuePair<string, TSqlFragment>>();
                    }
                }

                return ReturnSelf(predicate);
            }

            private static IEnumerable<KeyValuePair<string, TSqlFragment>> ReturnSelf(BooleanExpression predicate)
            {
                yield return new KeyValuePair<string, TSqlFragment>(predicate.GetFragmentCleanedText(), predicate);
            }

            private void DetectMatches(BooleanExpression predicate)
            {
                if (!appliedPredicates.Any())
                {
                    return;
                }

                ExplodePredicate(predicate)
                    .Join(appliedPredicates, _ => _.Key, _ => _.Key, (nested, outer) => new KeyValuePair<TSqlFragment, TSqlFragment>(nested.Value, outer.Value), StringComparer.OrdinalIgnoreCase)
                    .ToList()
                    .ForEach(match => ReportOn(match.Key, match.Value.StartLine));
            }

            private void ReportOn(TSqlFragment errorNode, int outerConditionLine)
            => Callback(errorNode, $"same seen at line {outerConditionLine}");
        }
    }
}
