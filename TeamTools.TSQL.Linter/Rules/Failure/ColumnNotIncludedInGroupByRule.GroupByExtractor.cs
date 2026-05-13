using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class ColumnNotIncludedInGroupByRule
    {
        private static IEnumerable<ScalarExpression> ExtractGroupingSpecification(IList<GroupingSpecification> grp)
        {
            for (int i = grp.Count - 1; i >= 0; i--)
            {
                foreach (var g in ExtractGroupingSpecification(grp[i]))
                {
                    yield return g;
                }
            }
        }

        private static IEnumerable<ScalarExpression> ExtractGroupingSpecification(GroupingSpecification grp)
        {
            if (grp is ExpressionGroupingSpecification grpExpr)
            {
                if (grpExpr.Expression is ValueExpression)
                {
                    // not interested in vars ans literals
                    return Enumerable.Empty<ScalarExpression>();
                }

                return Enumerable.Repeat(grpExpr.Expression, 1);
            }

            if (grp is RollupGroupingSpecification rollup)
            {
                return ExtractGroupingSpecification(rollup.Arguments);
            }

            if (grp is CubeGroupingSpecification cube)
            {
                return ExtractGroupingSpecification(cube.Arguments);
            }

            if (grp is CompositeGroupingSpecification composite)
            {
                return ExtractGroupingSpecification(composite.Items);
            }

            if (grp is GroupingSetsGroupingSpecification sets)
            {
                return ExtractGroupingSpecification(sets.Sets);
            }

            return Enumerable.Empty<ScalarExpression>();
        }

        private static HashSet<string> ExtractGroupedExpressions(IList<GroupingSpecification> groupby)
        {
            var groupedExpressions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = groupby.Count - 1; i >= 0; i--)
            {
                foreach (var groupedExpression in ExtractGroupingSpecification(groupby[i]))
                {
                    groupedExpressions.Add(groupedExpression.GetFragmentCleanedText());

                    if (groupedExpression is ColumnReferenceExpression colRef
                    && colRef.ColumnType != ColumnType.Wildcard)
                    {
                        string colName = colRef.MultiPartIdentifier.GetLastIdentifier().Value;

                        // TODO : validate column belonging
                        // also registering column name without alias
                        // because the rule currently is not able to check all the aliases
                        groupedExpressions.Add(colName);
                    }
                }
            }

            return groupedExpressions;
        }
    }
}
