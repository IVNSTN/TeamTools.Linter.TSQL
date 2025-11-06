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
    internal sealed class UidUniquenessQuestionedRule : AbstractRule
    {
        private static readonly string ColumnIsUniqueEnoughViolationTemplate = "{0} is {1}";
        private static readonly string IdentityAttributeName = "IDENTITY";
        private static readonly string NewIdAttributeName = "NEWID";
        private static readonly string UniqueAttributeName = "unique itself";

        public UidUniquenessQuestionedRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var elements = new TableDefinitionElementsEnumerator(node);

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
                    .Where(idx => idx.IsUnique && idx.ColumnsExceptPartitioned.Count > 1);
            }

            // if column is defined as unique itself
            var uniqueColumns = elements.Indices()
                .Where(idx => (idx is SqlIndexInfo ii)
                    && ii.IsUnique
                    && ii.Columns.Count == 1)
                .ToLookup(
                    idx => idx.TableName,
                    idx => idx.Columns.First().Name,
                    StringComparer.OrdinalIgnoreCase);

            // then mentioning it in more complex unique index or constraint is strange
            TableKeyColumnTypeValidator.CheckOnAllTables(
                elements,
                tbl => IndexFilter(tbl),
                (tbl, idx) => ValidateIndexedColumns(elements, uniqueColumns, tbl, idx));
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

        private void ValidateIndexedColumns(
            TableDefinitionElementsEnumerator elements,
            ILookup<string, string> uniqueColumns,
            string tableName,
            SqlTableElement idx)
        {
            var tblCols = elements.Tables[tableName].Columns;
            foreach (var col in idx.Columns.Where(c => tblCols.ContainsKey(c.Name)))
            {
                bool isUnique = uniqueColumns[tableName].Contains(col.Name, StringComparer.OrdinalIgnoreCase);

                if (!ValidateColumnNotAlreadyUnique(tblCols[col.Name], isUnique, out string violation))
                {
                    HandleNodeError(col.Reference, violation);
                }
            }
        }
    }
}
