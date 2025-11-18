using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0113", "INSERT_COL_COUNT")]
    internal sealed class InsertColumnCountRule : AbstractRule
    {
        public InsertColumnCountRule() : base()
        {
        }

        public override void Visit(InsertSpecification node)
        {
            if (node.InsertSource is ValuesInsertSource val)
            {
                if (val.RowValues.Count > 0)
                {
                    ValidateColCount(node.Columns, val.RowValues[0].ColumnValues);
                }
            }
            else if (node.InsertSource is SelectInsertSource sel)
            {
                ValidateColCount(node.Columns, sel.Select.GetQuerySpecification()?.SelectElements);
            }
        }

        public override void Visit(OutputIntoClause node) => ValidateColCount(node.IntoTableColumns, node.SelectColumns);

        public override void Visit(InsertMergeAction node) => ValidateColCount(node.Columns, node.Source.RowValues[0].ColumnValues);

        private static bool HasSelectStarItem<T>(IList<T> items)
        where T : TSqlFragment
        {
            int n = items.Count;
            for (int i = 0; i < n; i++)
            {
                if (items[i] is SelectStarExpression)
                {
                    return true;
                }
            }

            return false;
        }

        private static TSqlFragment ChooseBrokenElement<T>(IList<ColumnReferenceExpression> targetColumns, IList<T> sourceColumns)
        where T : TSqlFragment
        => targetColumns.Count > sourceColumns.Count ? targetColumns[targetColumns.Count - 1] as TSqlFragment : sourceColumns[sourceColumns.Count - 1];

        private void ValidateColCount<T>(IList<ColumnReferenceExpression> targetColumns, IList<T> sourceColumns)
        where T : TSqlFragment
        {
            if (targetColumns is null || sourceColumns is null
            || targetColumns.Count == 0 || sourceColumns.Count == 0)
            {
                // Could not determine or column list omitted
                return;
            }

            // TODO : Both, the computation and the problem are similar to UnionColumnCountRule implementation
            if (HasSelectStarItem(sourceColumns))
            {
                // SELECT * is hard to estimate col number
                return;
            }

            if (targetColumns.Count == sourceColumns.Count)
            {
                // everything is cool
                return;
            }

            HandleNodeError(ChooseBrokenElement(targetColumns, sourceColumns));
        }
    }
}
