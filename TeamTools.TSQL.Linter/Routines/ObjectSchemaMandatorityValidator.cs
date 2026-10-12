using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class ObjectSchemaMandatorityValidator
    {
        private readonly Action<TSqlFragment> callback;

        public ObjectSchemaMandatorityValidator(Action<TSqlFragment> callback)
        {
            this.callback = callback;
        }

        public void ValidateSchema(SchemaObjectName name, bool ignoreTempObjects = false)
        {
            if (name is null)
            {
                return;
            }

            if (ignoreTempObjects && name.IsTempTable())
            {
                // temp objects should not require schema since their scope is limited
                return;
            }

            if (!CheckSchemaIdentifier(name.SchemaIdentifier))
            {
                callback(name);
            }
        }

        private static bool CheckSchemaIdentifier(Identifier schemaIdentifier)
            => !string.IsNullOrEmpty(schemaIdentifier?.Value);
    }
}
