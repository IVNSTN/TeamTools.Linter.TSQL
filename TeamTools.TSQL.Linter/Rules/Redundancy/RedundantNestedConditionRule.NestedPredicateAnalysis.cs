using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Nested predicate analysis.
    /// </summary>
    internal partial class RedundantNestedConditionRule
    {
        private class DiveDeepPredicate
        {
            private readonly List<Tuple<string, TSqlFragment>> appliedPredicates = new List<Tuple<string, TSqlFragment>>();
            private readonly Action<TSqlFragment, string> callback;
            private readonly BooleanExpression predicate;
            private readonly TSqlFragment next;

            public DiveDeepPredicate(BooleanExpression predicate, TSqlFragment next, Action<TSqlFragment, string> callback)
            {
                this.predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
                this.next = next;
                this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            }

            public void Run()
            {
                // Analyzing given outer predicate parts
                Analyze(predicate, appliedPredicates);

                // Going deeper if nested statement is a conditional flow too
                AnalyzeNext(next, appliedPredicates);
            }

            private static IEnumerable<Tuple<string, TSqlFragment>> ExplodePredicate(BooleanExpression predicate)
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
                        return Enumerable.Empty<Tuple<string, TSqlFragment>>();
                    }
                }

                if (predicate is BooleanSurrogateExpression bse)
                {
                    return ReturnSurrogate(bse);
                }

                return ReturnSelf(predicate);
            }

            private static IEnumerable<Tuple<string, TSqlFragment>> ReturnSelf(BooleanExpression predicate)
            {
                yield return new Tuple<string, TSqlFragment>(predicate.GetFragmentCleanedText(), predicate);
            }

            private static IEnumerable<Tuple<string, TSqlFragment>> ReturnSurrogate(BooleanSurrogateExpression predicate)
            {
                // It has own GetFragmentCleanedText implementation
                yield return new Tuple<string, TSqlFragment>(predicate.GetSurrogateText(), predicate);
            }

            private static Tuple<string, TSqlFragment> GetFirstMatch(List<Tuple<string, TSqlFragment>> items, string searchValue)
            {
                for (int i = 0, n = items.Count; i < n; i++)
                {
                    var item = items[i];
                    if (item.Item1.Equals(searchValue, StringComparison.OrdinalIgnoreCase))
                    {
                        return item;
                    }
                }

                return default;
            }

            private void Analyze(BooleanExpression predicate, List<Tuple<string, TSqlFragment>> outerPredicates)
            {
                foreach (var p in ExplodePredicate(predicate))
                {
                    var dup = GetFirstMatch(outerPredicates, p.Item1);

                    if (dup != null)
                    {
                        ReportOn(p.Item2, dup.Item2.StartLine);
                    }
                    else
                    {
                        outerPredicates.Add(p);
                    }
                }

                predicate.Accept(new NestedBooleanExpressionsDetector(new Action<TSqlFragment>(nd => AnalyzeNext(nd, outerPredicates))));
            }

            private void AnalyzeNext(TSqlFragment predicate, ICollection<Tuple<string, TSqlFragment>> outerPredicates)
            {
                if (predicate is null)
                {
                    return;
                }

                while (predicate is BeginEndBlockStatement be)
                {
                    predicate = be.StatementList.Statements[0];
                }

                while (predicate is BooleanParenthesisExpression pe)
                {
                    predicate = pe.Expression;
                }

                if (predicate is BooleanExpression pbe)
                {
                    DetectMatches(pbe);
                }

                if (predicate is IfStatement pif)
                {
                    var nestedPredicates = new List<Tuple<string, TSqlFragment>>(outerPredicates);
                    Analyze(pif.Predicate, nestedPredicates);
                    AnalyzeNext(pif.ThenStatement, nestedPredicates);
                }

                if (predicate is WhileStatement pw)
                {
                    var nestedPredicates = new List<Tuple<string, TSqlFragment>>(outerPredicates);
                    Analyze(pw.Predicate, nestedPredicates);
                    AnalyzeNext(pw.Statement, nestedPredicates);
                }

                if (predicate is SearchedCaseExpression psc)
                {
                    int n = psc.WhenClauses.Count;
                    for (int i = 0; i < n; i++)
                    {
                        var option = psc.WhenClauses[i];
                        var nestedPredicates = new List<Tuple<string, TSqlFragment>>(outerPredicates);
                        Analyze(option.WhenExpression, nestedPredicates);
                        AnalyzeNext(option.ThenExpression, nestedPredicates);
                    }
                }

                if (predicate is IIfCall iif)
                {
                    var nestedPredicates = new List<Tuple<string, TSqlFragment>>(outerPredicates);
                    Analyze(iif.Predicate, nestedPredicates);
                    AnalyzeNext(iif.ThenExpression, nestedPredicates);
                }
            }

            private void DetectMatches(BooleanExpression predicate)
            {
                if (appliedPredicates.Count == 0)
                {
                    return;
                }

                var matches = ExplodePredicate(predicate)
                    .Join(
                        appliedPredicates,
                        _ => _.Item1,
                        _ => _.Item1,
                        (nested, outer) => new Tuple<TSqlFragment, TSqlFragment>(nested.Item2, outer.Item2),
                        StringComparer.OrdinalIgnoreCase);

                foreach (var match in matches)
                {
                    ReportOn(match.Item1, match.Item2.StartLine);
                }
            }

            private void ReportOn(TSqlFragment errorNode, int outerConditionLine)
            {
                callback(errorNode, string.Format(Strings.ViolationDetails_RedundantNestedConditionRule_DupPos, outerConditionLine.ToString()));
            }
        }

        private sealed class NestedBooleanExpressionsDetector : VisitorWithCallback
        {
            public NestedBooleanExpressionsDetector(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(IIfCall node) => Callback(node);

            public override void Visit(SearchedCaseExpression node) => Callback(node);
        }
    }
}
