using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor
{
    [ExcludeFromCodeCoverage]
    public class SqlColumnReferenceInfo
    {
        public SqlColumnReferenceInfo(string name, int position, TSqlFragment reference)
        {
            Name = name;
            Position = position;
            Reference = reference;
        }

        public string Name { get; }

        public int Position { get; }

        public TSqlFragment Reference { get; }
    }
}
