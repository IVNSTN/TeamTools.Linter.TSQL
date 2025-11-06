using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0775", "SPARSE_COL_INDEX_FILTER")]
    [IndexRule]
    internal sealed class SparseColumnIndexFilterRule : AbstractRule
    {
        public SparseColumnIndexFilterRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var info = new TableDefinitionElementsEnumerator(node);

            foreach (var tbl in info.Tables.Keys)
            {
                var sparseCols = info.Tables[tbl].Columns
                    .Where(col => col.Value.IsSparse)
                    .Select(col => col.Value.Name)
                    .ToList();

                if (!sparseCols.Any())
                {
                    continue;
                }

                var indices = info.Indices(tbl).OfType<SqlIndexInfo>().ToList();

                if (!indices.Any())
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
                return new List<BooleanExpression> { predicate };
            }

            return Enumerable.Empty<BooleanExpression>();
        }

        private static bool AllSparseColsHaveFilter(BooleanExpression predicate, ICollection<SqlColumnReferenceInfo> cols)
        {
            var filteredCols = ExplodePredicate(predicate)
                .OfType<BooleanIsNullExpression>()
                .Select(ex => ex.Expression)
                .OfType<ColumnReferenceExpression>()
                .Select(col => col.MultiPartIdentifier.Identifiers.Last().Value)
                .ToList();

            // all cols have IS NOT NULL predicate
            return !cols.Any(col => !filteredCols.Contains(col.Name, StringComparer.OrdinalIgnoreCase));
        }

        private void ValidateSparseColumnsIndexing(IList<string> sparseCols, IList<SqlIndexInfo> indices)
        {
            foreach (var idx in indices)
            {
                var matchedCols = idx.Columns
                    .Join(sparseCols, idxCol => idxCol.Name, _ => _, (c1, _) => c1, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!matchedCols.Any())
                {
                    continue;
                }

                BooleanExpression predicate;

                if (idx.Definition is IndexDefinition ix)
                {
                    predicate = ix.FilterPredicate;
                    HandleNodeError(matchedCols[0].Reference, $"{matchedCols[0].Name} in {idx.Name}");
                }
                else if (idx.Definition is CreateIndexStatement crix)
                {
                    predicate = crix.FilterPredicate;
                }
                else
                {
                    continue;
                }

                if (!AllSparseColsHaveFilter(predicate, matchedCols))
                {
                    HandleNodeError(matchedCols[0].Reference, $"{matchedCols[0].Name} in {idx.Name}");
                }
            }
        }
    }
}
