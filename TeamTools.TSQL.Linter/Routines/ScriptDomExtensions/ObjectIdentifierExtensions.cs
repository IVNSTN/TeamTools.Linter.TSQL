using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    public static class ObjectIdentifierExtensions
    {
        public static bool IsTempTable(this SchemaObjectName name)
        {
            return IsTempTable(name.BaseIdentifier.Value);
        }

        public static bool IsTempTable(string name) => TSqlDomainAttributes.IsTempObject(name);
    }
}
