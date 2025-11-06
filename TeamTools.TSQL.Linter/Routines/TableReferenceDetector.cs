using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class TableReferenceDetector : TSqlFragmentVisitor
    {
        private readonly IDictionary<string, TSqlFragment> tableReferences;
        private readonly Action<string> callback = null;

        public TableReferenceDetector(IDictionary<string, TSqlFragment> tableReferences)
        {
            this.tableReferences = tableReferences;
        }

        public TableReferenceDetector(IDictionary<string, TSqlFragment> tableReferences, Action<string> callback)
        {
            this.tableReferences = tableReferences;
            this.callback = callback;
        }

        public override void Visit(NamedTableReference node)
            => RegisterTableReference(node.SchemaObject.GetFullName(), node);

        public override void Visit(VariableTableReference node)
            => RegisterTableReference(node.Variable.Name, node);

        private void RegisterTableReference(string tableName, TSqlFragment node)
        {
            if (string.IsNullOrEmpty(tableName) || node is null)
            {
                return;
            }

            tableReferences.TryAdd(tableName, node);

            callback?.Invoke(tableName);
        }
    }
}
