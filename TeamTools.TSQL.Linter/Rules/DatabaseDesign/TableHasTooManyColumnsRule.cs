using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0826", "TOO_MANY_COLUMNS")]
    internal sealed class TableHasTooManyColumnsRule : ScriptAnalysisServiceConsumingRule
    {
        private static readonly int MaxAllowedCols = 80; // TODO : needs some motivation

        public TableHasTooManyColumnsRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var tblInfo = GetService<TableDefinitionElementsEnumerator>(node);

            if (tblInfo.Tables.Count == 0)
            {
                return;
            }

            var wideTables =
                from tbl in tblInfo.Tables
                where !tblInfo.Indices(tbl.Key).OfType<SqlIndexInfo>().Any(ix => ix.IsColumnStore)
                let colCount = tbl.Value.Columns.Count(col => !col.Value.IsSparse)
                where colCount > MaxAllowedCols
                let colNode = tbl.Value.Node.ColumnDefinitions[tbl.Value.Node.ColumnDefinitions.Count - 1].ColumnIdentifier
                select new Tuple<string, int, TSqlFragment>(tbl.Key, colCount, colNode);

            foreach (var tbl in wideTables)
            {
                HandleNodeError(tbl.Item3, $"{tbl.Item1}({tbl.Item2.ToString()}");
            }
        }
    }
}
