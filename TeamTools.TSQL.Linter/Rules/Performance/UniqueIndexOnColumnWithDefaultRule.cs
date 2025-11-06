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
    internal sealed class UniqueIndexOnColumnWithDefaultRule : AbstractRule
    {
        // TODO : what about not unique indexes? still filtered would be better
        public UniqueIndexOnColumnWithDefaultRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var idxVisitor = new TableIndicesVisitor();
            node.Accept(idxVisitor);

            if (idxVisitor.Indices.Count == 0)
            {
                return;
            }

            ValidateIndexes(idxVisitor);
        }

        private void DoGrabColumnNameFromExpression(TSqlFragment predicate, IList<string> filteredCols)
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
                filteredCols.Add(col.MultiPartIdentifier.Identifiers.Last().Value);
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
            var uniqueConstraints = new List<KeyValuePair<TSqlFragment, IList<ColumnWithSortOrder>>>();

            uniqueConstraints.AddRange(
                idxVisitor.Table.Definition.TableConstraints
                    .OfType<UniqueConstraintDefinition>()
                    .Where(uq => !(uq.Clustered ?? (uq.IndexType?.IndexTypeKind == IndexTypeKind.ClusteredColumnStore)) && !uq.IsPrimaryKey)
                    .Select(uq => new KeyValuePair<TSqlFragment, IList<ColumnWithSortOrder>>(uq, uq.Columns)));

            uniqueConstraints.AddRange(
                idxVisitor.Table.Definition.ColumnDefinitions
                    .SelectMany(col => col.Constraints.OfType<UniqueConstraintDefinition>())
                    .Select(uq => new KeyValuePair<TSqlFragment, IList<ColumnWithSortOrder>>(uq, uq.Columns)));

            uniqueConstraints.AddRange(
                idxVisitor.Indices
                    .Where(idx => idx.Unique && !(idx.Clustered ?? false))
                    .Select(uq => new KeyValuePair<TSqlFragment, IList<ColumnWithSortOrder>>(uq.Definition, uq.Columns)));

            if (uniqueConstraints.Count == 0)
            {
                return;
            }

            // detecting nullable cols and cols with defaults
            var lowSelectiveCols = new List<ColumnDefinition>();
            lowSelectiveCols.AddRange(
                idxVisitor.Table.Definition.ColumnDefinitions.Where(col =>
                    col.ComputedColumnExpression == null
                    && col.IdentityOptions == null
                    && (col.DefaultConstraint != null || (col.Constraints.OfType<NullableConstraintDefinition>().FirstOrDefault()?.Nullable ?? true))));

            if (lowSelectiveCols.Count == 0)
            {
                return;
            }

            // catching unique indices with low selective cols with no filter on those cols
            foreach (var uq in uniqueConstraints)
            {
                // detecting low selective cols in index definition
                List<string> badColsInIndex = uq.Value
                    .Select(col => col.Column.MultiPartIdentifier.Identifiers.Last().Value)
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
                if (uq.Key is CreateIndexStatement idxStmt && idxStmt.FilterPredicate != null)
                {
                    predicate = idxStmt.FilterPredicate;
                }
                else if (uq.Key is IndexDefinition idxDef && idxDef.FilterPredicate != null)
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

                HandleNodeError(uq.Key, string.Join(",", badColsInIndex));
            }
        }
    }
}
