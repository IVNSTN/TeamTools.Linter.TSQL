using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0703", "BAD_TYPE_FOR_COLUMNSTORE")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    [IndexRule]
    internal sealed class BadTypeForColumnstoreIndexRule : ScriptAnalysisServiceConsumingRule
    {
        private static readonly int MaxColsPerReport = 7;
        private static readonly string ColSeparator = ", ";
        private static readonly string ColInfoOutputTemplate = "{0} {1}";

        private static readonly HashSet<string> BadTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "HIERARCHYID",
            "IMAGE",
            "NTEXT",
            "ROWVERSION",
            "SQL_VARIANT",
            "TEXT",
            "TIMESTAMP",
            "XML",
        };

        private readonly Func<string, bool> isBadType = new Func<string, bool>(BadTypes.Contains);

        public BadTypeForColumnstoreIndexRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var elements = GetService<TableDefinitionElementsEnumerator>(node);

            if (elements.Tables.Count == 0)
            {
                return;
            }

            Func<string, IEnumerable<SqlIndexInfo>> filterColumnStoreIndexes = new Func<string, IEnumerable<SqlIndexInfo>>(tbl => IsColumnStored(elements, tbl));
            Action<string, SqlTableElement> validateColType = new Action<string, SqlTableElement>((tbl, el) => ValidateColumnType(el, elements.Tables[tbl]));

            TableKeyColumnTypeValidator.CheckOnAllTables(
                elements,
                filterColumnStoreIndexes,
                validateColType);
        }

        private static IEnumerable<SqlIndexInfo> IsColumnStored(TableDefinitionElementsEnumerator elements, string tbl)
            => elements.Indices(tbl).OfType<SqlIndexInfo>().Where(idx => idx.IsColumnStore);

        private void ValidateColumnType(SqlTableElement el, SqlTableDefinition def)
        {
            // COLUMNSTORE index definition does not support listing columns
            // el.Columns.Count is always 0. Thus going directly to table definition.
            var badCols = def.Columns
                .Where(col => isBadType(col.Value.TypeName))
                // columnstore index can contain really many columns
                .Take(MaxColsPerReport)
                .Select(col => string.Format(ColInfoOutputTemplate, col, col.Value.TypeName))
                .ToArray();

            if (badCols.Length == 0)
            {
                return;
            }

            HandleNodeError(el.Definition, string.Join(ColSeparator, badCols));
        }
    }
}
