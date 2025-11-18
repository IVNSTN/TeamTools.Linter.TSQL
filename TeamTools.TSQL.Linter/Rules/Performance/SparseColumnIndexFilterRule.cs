using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0775", "SPARSE_COL_INDEX_FILTER")]
    [IndexRule]
    internal sealed class SparseColumnIndexFilterRule : ScriptAnalysisServiceConsumingRule
    {
        public SparseColumnIndexFilterRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var info = GetService<TableDefinitionElementsEnumerator>(node);

            if (info.Tables.Count == 0)
            {
                return;
            }

            foreach (var tbl in info.Tables)
            {
                var sparseCols = tbl.Value.Columns
                    .Where(col => col.Value.IsSparse)
                    .Select(col => col.Value.Name)
                    .ToArray();

                if (sparseCols.Length == 0)
                {
                    continue;
                }

                var indices = info.Indices(tbl.Key).OfType<SqlIndexInfo>().ToArray();

                if (indices.Length == 0)
                {
                    continue;
                }

                ValidateSparseColumnsIndexing(sparseCols, indices);
            }
        }

        // TODO : make it more accurate
        private static IEnumerable<BooleanExpression> ExplodePredicate(BooleanExpression predicate, bool negated = false)
        {
            while (predicate is BooleanParenthesisExpression pe)
            {
                predicate = pe.Expression;
            }

            if (predicate is BooleanNotExpression bn)
            {
                return ExplodePredicate(bn.Expression, !negated);
            }

            // ORs are hard to understand in code
            if (predicate is BooleanBinaryExpression bin && bin.BinaryExpressionType == BooleanBinaryExpressionType.And)
            {
                return ExplodePredicate(bin.FirstExpression)
                    .Union(ExplodePredicate(bin.SecondExpression));
            }

            if (predicate is BooleanIsNullExpression isn && (isn.IsNot ^ negated)
            && isn.Expression is ColumnReferenceExpression)
            {
                return Enumerable.Repeat(predicate, 1);
            }

            return Enumerable.Empty<BooleanExpression>();
        }

        private static bool AllSparseColsHaveFilter(BooleanExpression predicate, ICollection<SqlColumnReferenceInfo> cols)
        {
            var filteredCols = new HashSet<string>(
                ExplodePredicate(predicate)
                    .OfType<BooleanIsNullExpression>()
                    .Select(ex => ex.Expression)
                    .OfType<ColumnReferenceExpression>()
                    .Select(col => col.MultiPartIdentifier.GetLastIdentifier().Value),
                StringComparer.OrdinalIgnoreCase);

            // all cols have IS NOT NULL predicate
            return !cols.Any(col => !filteredCols.Contains(col.Name));
        }

        private void ValidateSparseColumnsIndexing(string[] sparseCols, SqlIndexInfo[] indices)
        {
            foreach (var idx in indices)
            {
                var matchedCols = idx.Columns
                    .Join(sparseCols, idxCol => idxCol.Name, _ => _, (c1, _) => c1, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (matchedCols.Length == 0)
                {
                    continue;
                }

                BooleanExpression predicate;

                var firstMatch = matchedCols[0];

                if (idx.Definition is IndexDefinition ix)
                {
                    predicate = ix.FilterPredicate;
                }
                else if (idx.Definition is CreateIndexStatement crix)
                {
                    predicate = crix.FilterPredicate;
                }
                else
                {
                    // Unique constraint as well as primary key cannot be filtered
                    continue;
                }

                if (!AllSparseColsHaveFilter(predicate, matchedCols))
                {
                    ReportViolation(firstMatch, idx.Name);
                }
            }
        }

        private void ReportViolation(SqlColumnReferenceInfo badCol, string indexName)
        {
            HandleNodeError(badCol.Reference, string.Format(Strings.ViolationDetails_SparseColumnIndexFilterRule_ColInIndex, badCol.Name, indexName));
        }
    }
}
