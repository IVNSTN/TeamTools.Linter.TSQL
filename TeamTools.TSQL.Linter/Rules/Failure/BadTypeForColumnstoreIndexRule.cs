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
    internal sealed class BadTypeForColumnstoreIndexRule : AbstractRule
    {
        private static readonly int MaxColsPerReport = 7;
        private static readonly string ColSeparator = ", ";
        private static readonly string ColInfoOutputTemplate = "{0} {1}";

        private static readonly ICollection<string> BadTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        static BadTypeForColumnstoreIndexRule()
        {
            BadTypes.Add("dbo.NTEXT");
            BadTypes.Add("dbo.TEXT");
            BadTypes.Add("dbo.IMAGE");
            BadTypes.Add("dbo.ROWVERSION");
            BadTypes.Add("dbo.TIMESTAMP");
            BadTypes.Add("dbo.SQL_VARIANT");
            BadTypes.Add("dbo.XML");
            BadTypes.Add("dbo.HIERARCHYID");
        }

        public BadTypeForColumnstoreIndexRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var elements = new TableDefinitionElementsEnumerator(node);

            TableKeyColumnTypeValidator.CheckOnAllTables(
                elements,
                tbl => elements.Indices(tbl).OfType<SqlIndexInfo>().Where(idx => idx.IsColumnStore),
                (tbl, el) => ValidateColumnType(el, elements.Tables[tbl]));
        }

        private void ValidateColumnType(SqlTableElement el, SqlTableDefinition def)
        {
            // COLUMNSTORE index definition does not support listing columns
            // el.Columns.Count is always 0. Thus going directly to table definition.
            var badCols = def.Columns.Keys
                .Where(col => BadTypes.Contains(def.Columns[col].TypeName))
                // columnstore index can contain really many columns
                .Take(MaxColsPerReport)
                .Select(col => string.Format(ColInfoOutputTemplate, col, def.Columns[col].TypeName))
                .ToList();

            if (!badCols.Any())
            {
                return;
            }

            HandleNodeError(el.Definition, string.Join(ColSeparator, badCols));
        }
    }
}
