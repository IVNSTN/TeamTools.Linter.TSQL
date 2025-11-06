using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class TableCreationDetector : TSqlFragmentVisitor
    {
        private readonly bool withTriggerTables = true;
        private readonly IDictionary<string, TSqlFragment> tables = new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

        public TableCreationDetector(bool withTriggerTables = false)
        {
            this.withTriggerTables = withTriggerTables;
        }

        public IDictionary<string, TSqlFragment> Tables => tables;

        public override void Visit(CreateTableStatement node)
        {
            string tableName = node.SchemaObjectName.GetFullName();
            Tables.TryAdd(tableName, node);
        }

        public override void Visit(DeclareTableVariableStatement node)
        {
            string tableName = node.Body.VariableName.Value;
            Tables.TryAdd(tableName, node);
        }

        public override void Visit(TriggerStatementBody node)
        {
            if (!withTriggerTables)
            {
                return;
            }

            Tables.TryAdd(TSqlDomainAttributes.TriggerSystemTables.Inserted, node.TriggerObject);
            Tables.TryAdd(TSqlDomainAttributes.TriggerSystemTables.Deleted, node.TriggerObject);
        }
    }
}
