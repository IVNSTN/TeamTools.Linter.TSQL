using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0723", "ALIAS_MISSES_COL_POSITION")]
    internal sealed class MismatchingColumnAliasPositionRule : AbstractRule
    {
        private static readonly int MaxAliasesToReport = 3;

        public MismatchingColumnAliasPositionRule() : base()
        {
        }

        public override void Visit(CommonTableExpression node)
            => ValidateAliasesPositions(node.Columns, node.QueryExpression.GetQuerySpecification()?.SelectElements, ViolationHandlerWithMessage);

        public override void Visit(QueryDerivedTable node)
            => ValidateAliasesPositions(node.Columns, node.QueryExpression.GetQuerySpecification()?.SelectElements, ViolationHandlerWithMessage);

        private static void ValidateAliasesPositions(IList<Identifier> aliases, IList<SelectElement> columns, Action<TSqlFragment, string> callback)
        {
            if (aliases?.Count == 0 || columns?.Count == 0
            || columns.OfType<SelectStarExpression>().Any())
            {
                return;
            }

            var outputAliases = ExtractColumnNames(aliases).ToList();
            var sourceColumns = ExtractColumnNames(columns).ToList();

            if (outputAliases.Count != sourceColumns.Count)
            {
                // Mismatching number of columns is handled by a separate rule
                return;
            }

            var aliasPosMismatch = MatchPositions(outputAliases, sourceColumns).ToList();

            if (aliasPosMismatch.Count == 0)
            {
                return;
            }

            var suspiciousAliasList = string.Join(
                ", ",
                aliasPosMismatch
                    .Select(i => outputAliases[i])
                    .Take(MaxAliasesToReport));

            callback(aliases[aliasPosMismatch[0]], suspiciousAliasList);
        }

        private static IEnumerable<int> MatchPositions(List<string> outputAliases, List<string> sourceColumns)
        {
            int n = outputAliases.Count;
            for (int i = 0; i < n; i++)
            {
                int colIndex = sourceColumns.IndexOf(outputAliases[i]);
                if (colIndex >= 0 && colIndex != i)
                {
                    yield return i;
                }
            }
        }

        private static IEnumerable<string> ExtractColumnNames(IList<Identifier> columns)
        {
            int n = columns.Count;
            for (int i = 0; i < n; i++)
            {
                yield return columns[i].Value;
            }
        }

        private static IEnumerable<string> ExtractColumnNames(IList<SelectElement> columns)
        {
            int n = columns.Count;
            for (int i = 0; i < n; i++)
            {
                yield return GetColumnName(columns[i]);
            }
        }

        private static string GetColumnName(SelectElement col)
        {
            if (col is SelectScalarExpression expr)
            {
                if (expr.ColumnName is null && expr.Expression is ColumnReferenceExpression colRef)
                {
                    return colRef.MultiPartIdentifier?.GetLastIdentifier().Value;
                }

                return expr.ColumnName?.Identifier?.Value;
            }

            return default;
        }
    }
}
