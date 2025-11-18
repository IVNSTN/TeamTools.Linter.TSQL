using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0910", "INDEXING_COL_WITH_DEFAULT")]
    [IndexRule]
    internal sealed class UniqueIndexOnColumnWithDefaultRule : ScriptAnalysisServiceConsumingRule
    {
        // TODO : what about not unique indexes? still filtered would be better
        public UniqueIndexOnColumnWithDefaultRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var idxVisitor = GetService<TableIndicesVisitor>(node);

            if (idxVisitor.Indices.Count == 0)
            {
                return;
            }

            ValidateIndexes(idxVisitor);
        }

        private void DoGrabColumnNameFromExpression(TSqlFragment predicate, List<string> filteredCols)
        {
            if (predicate is BooleanBinaryExpression bin)
            {
                DoGrabColumnNameFromExpression(bin.FirstExpression, filteredCols);
                DoGrabColumnNameFromExpression(bin.SecondExpression, filteredCols);
            }
            else if (predicate is BooleanParenthesisExpression par)
            {
                DoGrabColumnNameFromExpression(par.Expression, filteredCols);
            }
            else if (predicate is BooleanNotExpression nott)
            {
                DoGrabColumnNameFromExpression(nott.Expression, filteredCols);
            }
            else if (predicate is BooleanComparisonExpression comp)
            {
                DoGrabColumnNameFromExpression(comp.FirstExpression, filteredCols);
                DoGrabColumnNameFromExpression(comp.SecondExpression, filteredCols);
            }
            else if (predicate is BooleanIsNullExpression isnull)
            {
                DoGrabColumnNameFromExpression(isnull.Expression, filteredCols);
            }
            else if (predicate is ColumnReferenceExpression col)
            {
                filteredCols.Add(col.MultiPartIdentifier.GetLastIdentifier().Value);
            }
        }

        // TODO : grab the value against which column is compared. and comparison type
        private void ExtractColNamesFromPredicate(BooleanExpression predicate, out List<string> filteredCols)
        {
            filteredCols = new List<string>();

            DoGrabColumnNameFromExpression(predicate, filteredCols);
            // TODO : rem dups
        }

        // TODO : refactoring needed
        private void ValidateIndexes(TableIndicesVisitor idxVisitor)
        {
            // detecting unique indexes and constraints on any columns
            var uniqueConstraints = new List<Tuple<TSqlFragment, IList<ColumnWithSortOrder>>>();

            uniqueConstraints.AddRange(
                idxVisitor.Table.Definition.TableConstraints
                    .OfType<UniqueConstraintDefinition>()
                    .Where(uq => !(uq.Clustered ?? (uq.IndexType?.IndexTypeKind == IndexTypeKind.ClusteredColumnStore)) && !uq.IsPrimaryKey)
                    .Select(uq => new Tuple<TSqlFragment, IList<ColumnWithSortOrder>>(uq, uq.Columns)));

            uniqueConstraints.AddRange(
                idxVisitor.Table.Definition.ColumnDefinitions
                    .SelectMany(col => col.Constraints.OfType<UniqueConstraintDefinition>())
                    .Select(uq => new Tuple<TSqlFragment, IList<ColumnWithSortOrder>>(uq, uq.Columns)));

            uniqueConstraints.AddRange(
                idxVisitor.Indices
                    .Where(idx => idx.Unique && !(idx.Clustered ?? false))
                    .Select(uq => new Tuple<TSqlFragment, IList<ColumnWithSortOrder>>(uq.Definition, uq.Columns)));

            if (uniqueConstraints.Count == 0)
            {
                return;
            }

            // detecting nullable cols and cols with defaults
            var lowSelectiveCols = new List<ColumnDefinition>();
            lowSelectiveCols.AddRange(
                idxVisitor.Table.Definition.ColumnDefinitions.Where(col =>
                    col.ComputedColumnExpression is null
                    && col.IdentityOptions is null
                    && (col.DefaultConstraint != null || (col.Constraints.OfType<NullableConstraintDefinition>().FirstOrDefault()?.Nullable ?? true))));

            if (lowSelectiveCols.Count == 0)
            {
                return;
            }

            // catching unique indices with low selective cols with no filter on those cols
            int n = uniqueConstraints.Count;
            for (int i = 0; i < n; i++)
            {
                var uq = uniqueConstraints[i];

                // detecting low selective cols in index definition
                List<string> badColsInIndex = uq.Item2
                    .ExtractNames()
                    .Where(idxCol =>
                        lowSelectiveCols.Exists(tblCol =>
                            string.Equals(tblCol.ColumnIdentifier.Value, idxCol, StringComparison.OrdinalIgnoreCase)))
                    .Distinct()
                    .ToList();

                if (badColsInIndex.Count == 0)
                {
                    continue;
                }

                // ignoring filtered columns
                BooleanExpression predicate = default;
                if (uq.Item1 is CreateIndexStatement idxStmt && idxStmt.FilterPredicate != null)
                {
                    predicate = idxStmt.FilterPredicate;
                }
                else if (uq.Item1 is IndexDefinition idxDef && idxDef.FilterPredicate != null)
                {
                    predicate = idxDef.FilterPredicate;
                }

                if (predicate != null)
                {
                    // TODO : check that predicate compares exactly with default/null value of a column
                    ExtractColNamesFromPredicate(predicate, out List<string> filteredCols);
                    badColsInIndex.RemoveAll(col => filteredCols.Contains(col));
                }

                if (badColsInIndex.Count == 0)
                {
                    continue;
                }

                HandleNodeError(uq.Item1, string.Join(",", badColsInIndex));
            }
        }
    }
}
