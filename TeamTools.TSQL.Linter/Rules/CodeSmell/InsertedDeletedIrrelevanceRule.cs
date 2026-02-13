using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0843", "OUTPUT_MISMATCHES_ACTION")]
    internal sealed class InsertedDeletedIrrelevanceRule : AbstractRule
    {
        public InsertedDeletedIrrelevanceRule() : base()
        {
        }

        public override void ExplicitVisit(InsertSpecification node)
        {
            var cols = node.OutputClause?.SelectColumns ?? node.OutputIntoClause?.SelectColumns;
            Detect(TSqlDomainAttributes.TriggerSystemTables.Deleted, cols);
        }

        public override void ExplicitVisit(DeleteSpecification node)
        {
            var cols = node.OutputClause?.SelectColumns ?? node.OutputIntoClause?.SelectColumns;
            Detect(TSqlDomainAttributes.TriggerSystemTables.Inserted, cols);
        }

        public override void ExplicitVisit(MergeSpecification node)
        {
            bool hasInsert = false;
            bool hasDelete = false;

            for (int i = 0, n = node.ActionClauses.Count; i < n; i++)
            {
                var act = node.ActionClauses[i].Action;

                if (act is InsertMergeAction)
                {
                    hasInsert = true;
                }
                else if (act is DeleteMergeAction)
                {
                    hasDelete = true;
                }
                else if (act is UpdateMergeAction)
                {
                    hasInsert = true;
                    hasDelete = true;
                }
            }

            if (hasInsert && hasDelete)
            {
                // Both INSERTED and DELETED may contain data
                return;
            }

            var irrelevantTbl = hasInsert ? TSqlDomainAttributes.TriggerSystemTables.Deleted : TSqlDomainAttributes.TriggerSystemTables.Inserted;
            var cols = node.OutputClause?.SelectColumns ?? node.OutputIntoClause?.SelectColumns;
            Detect(irrelevantTbl, cols);
        }

        private static Identifier GetColumnIdentifier(SelectElement element)
        {
            if (element is SelectStarExpression star)
            {
                if (star.Qualifier.Count == 1)
                {
                    // <tbl>.*
                    return star.Qualifier[0];
                }
            }
            else if (element is SelectScalarExpression expr && expr.Expression is ColumnReferenceExpression col)
            {
                if (col.MultiPartIdentifier?.Count == 2)
                {
                    // <tbl>.<col>
                    return col.MultiPartIdentifier[0];
                }
            }

            return default;
        }

        private void Detect(string irrelevantEventTable, IList<SelectElement> selectedCols)
        {
            if (selectedCols is null || selectedCols.Count == 0)
            {
                return;
            }

            for (int i = selectedCols.Count - 1; i >= 0; i--)
            {
                var colSource = GetColumnIdentifier(selectedCols[i]);
                if (colSource != null && string.Equals(colSource.Value, irrelevantEventTable, StringComparison.OrdinalIgnoreCase))
                {
                    HandleNodeError(colSource, irrelevantEventTable);
                }
            }
        }
    }
}
