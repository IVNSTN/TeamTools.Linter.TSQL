using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0701", "UID_UNIQ_QUESTIONED")]
    internal sealed class UidUniquenessQuestionedRule : ScriptAnalysisServiceConsumingRule
    {
        private static readonly string ColumnIsUniqueEnoughViolationTemplate = "{0} is {1}";
        private static readonly string IdentityAttributeName = "IDENTITY";
        private static readonly string NewIdAttributeName = "NEWID";
        private static readonly string UniqueAttributeName = "unique itself";
        private readonly Func<SqlIndexInfo, bool> filterIdxForNonPartitionedColCount;
        private readonly Func<SqlIndexInfo, bool> filterUniqueIdxForTotalColCount;

        public UidUniquenessQuestionedRule() : base()
        {
            filterIdxForNonPartitionedColCount = new Func<SqlIndexInfo, bool>(idx => idx.IsUnique && idx.ColumnsExceptPartitioned.Count > 1);
            filterUniqueIdxForTotalColCount = new Func<SqlIndexInfo, bool>(idx => idx.IsUnique && idx.Columns.Count == 1);
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var elements = GetService<TableDefinitionElementsEnumerator>(node);

            if (elements.Tables.Count == 0)
            {
                return;
            }

            IEnumerable<SqlIndexInfo> IndexFilter(string tbl)
            {
                // table variables don't provide much indexing
                // possibilities thus including something additional
                // into PK might be useful
                // TODO : not sure that excluding this case is a good thing actually
                if (elements.Tables[tbl].TableType == SqlTableType.TableVariable)
                {
                    return Enumerable.Empty<SqlIndexInfo>();
                }

                return elements.Indices(tbl)
                    .OfType<SqlIndexInfo>()
                    // index is unique and contains more than one column
                    // partitioned cols are not counted
                    .Where(filterIdxForNonPartitionedColCount);
            }

            // if column is defined as unique itself
            var uniqueColumns = elements.Indices()
                .OfType<SqlIndexInfo>()
                .Where(filterUniqueIdxForTotalColCount)
                .ToLookup(
                    idx => idx.TableName,
                    idx => idx.Columns[0].Name,
                    StringComparer.OrdinalIgnoreCase);

            Func<string, IEnumerable<SqlIndexInfo>> filterIndexes = new Func<string, IEnumerable<SqlIndexInfo>>(IndexFilter);
            Action<string, SqlTableElement> validateIndexCols = new Action<string, SqlTableElement>((tbl, idx) => ValidateIndexedColumns(elements, uniqueColumns, tbl, idx, ViolationHandlerWithMessage));

            // then mentioning it in more complex unique index or constraint is strange
            TableKeyColumnTypeValidator.CheckOnAllTables(
                elements,
                filterIndexes,
                validateIndexCols);
        }

        private static bool ValidateColumnNotAlreadyUnique(
            SqlColumnInfo col,
            bool isUnique,
            out string violation)
        {
            if (col.IsIdentity)
            {
                violation = IdentityAttributeName;
            }
            else if (col.IsNewId)
            {
                violation = NewIdAttributeName;
            }
            else if (isUnique)
            {
                violation = UniqueAttributeName;
            }
            else
            {
                violation = "";
            }

            if (string.IsNullOrEmpty(violation))
            {
                return true;
            }

            violation = string.Format(ColumnIsUniqueEnoughViolationTemplate, col.Name, violation);
            return false;
        }

        private static void ValidateIndexedColumns(
            TableDefinitionElementsEnumerator elements,
            ILookup<string, string> uniqueColumns,
            string tableName,
            SqlTableElement idx,
            Action<TSqlFragment, string> callback)
        {
            var tblCols = elements.Tables[tableName].Columns;

            int n = idx.Columns.Count;
            for (int i = 0; i < n; i++)
            {
                var col = idx.Columns[i];
                if (!tblCols.TryGetValue(col.Name, out var colInfo))
                {
                    continue;
                }

                if (!ValidateColumnNotAlreadyUnique(colInfo, uniqueColumns[tableName].Contains(col.Name), out string violation))
                {
                    callback.Invoke(col.Reference, violation);
                }
            }
        }
    }
}
