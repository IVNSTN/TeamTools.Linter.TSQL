using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor
{
    [ExcludeFromCodeCoverage]
    public class SqlTableElement
    {
        public SqlTableElement(
            string tableName,
            SqlTableElementType elementType,
            string name,
            List<SqlColumnReferenceInfo> columns,
            TSqlFragment definition)
        {
            TableName = tableName;
            ElementType = elementType;
            Name = name;
            Columns = columns;
            Definition = definition;
        }

        public SqlTableElementType ElementType { get; }

        public string TableName { get; }

        public string Name { get; }

        public List<SqlColumnReferenceInfo> Columns { get; }

        public TSqlFragment Definition { get; }
    }
}
