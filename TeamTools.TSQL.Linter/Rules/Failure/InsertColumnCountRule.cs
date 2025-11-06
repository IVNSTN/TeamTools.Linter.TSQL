using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
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

        public override void Visit(InsertSpecification node) => ValidateColCount(node.Columns, GetInsertedElements(node.InsertSource)?.ToList());

        public override void Visit(OutputIntoClause node) => ValidateColCount(node.IntoTableColumns, node.SelectColumns);

        public override void Visit(InsertMergeAction node) => ValidateColCount(node.Columns, node.Source.RowValues[0].ColumnValues);

        private static IEnumerable<TSqlFragment> GetInsertedElements(InsertSource node)
        {
            if (node is ValuesInsertSource val && val.RowValues.Count > 0)
            {
                return val.RowValues[0].ColumnValues;
            }

            if (node is SelectInsertSource sel)
            {
                return sel.Select.GetQuerySpecification()?.SelectElements;
            }

            return default;
        }

        private static TSqlFragment ChooseBrokenElement<T>(IList<ColumnReferenceExpression> targetColumns, IList<T> sourceColumns)
        where T : TSqlFragment
        => targetColumns.Count > sourceColumns.Count ? targetColumns.Last() as TSqlFragment : sourceColumns.Last();

        private void ValidateColCount<T>(IList<ColumnReferenceExpression> targetColumns, IList<T> sourceColumns)
        where T : TSqlFragment
        {
            if (targetColumns is null || sourceColumns is null
            || targetColumns.Count <= 0 || sourceColumns.Count <= 0)
            {
                // Could not determine or column list omitted
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
