using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class TempTableCachingRule
    {
        // Reasons for non-cacheable temp tables:
        // - temp table name reused
        // - named constraint
        // - DDL over table after creation including
        //      - alter table
        //      - create index
        //      - create statistics
        // Docs:
        //      - https://www.sql.kiwi/2012/08/temporary-object-caching-explained/
        //      - https://learn.microsoft.com/en-us/shows/sql-workshops/temp-table-caching-in-sql-server
        private sealed class TempTableVisitor : TSqlFragmentVisitor
        {
            public TempTableVisitor(Action<TSqlFragment, string> callback)
            {
                Callback = callback;
            }

            public HashSet<string> TempTables { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            private Action<TSqlFragment, string> Callback { get; }

            // Table variables are very similar to temp tables and can be cached as well
            public override void ExplicitVisit(DeclareTableVariableBody node)
            {
                RegisterTempTable(node.VariableName);
                node.Definition.AcceptChildren(this);
            }

            public override void ExplicitVisit(CreateTableStatement node)
            {
                if (!IsTempTable(node.SchemaObjectName))
                {
                    return;
                }

                RegisterTempTable(node.SchemaObjectName);
                node.Definition.AcceptChildren(this);
            }

            // ExplicitVisit to prevent visiting ConstraintDefinition defined
            // in an ALTER of non-temp tables
            public override void ExplicitVisit(AlterTableAddTableElementStatement node)
            {
                if (!IsTempTable(node.SchemaObjectName))
                {
                    return;
                }

                DetectTempTableDdl(node.SchemaObjectName);
                node.Definition.AcceptChildren(this);
            }

            // Visiting any kind of constraint
            public override void Visit(ConstraintDefinition node)
            {
                if (node.ConstraintIdentifier != null)
                {
                    // TODO : move message to resources, add translations
                    // named constraint
                    Callback(node.ConstraintIdentifier, "named constraint");
                }
            }

            // Any DDL on a temp table prevents its caching
            public override void Visit(AlterTableStatement node) => DetectTempTableDdl(node.SchemaObjectName);

            public override void Visit(IndexStatement node) => DetectTempTableDdl(node.OnName);

            public override void Visit(CreateStatisticsStatement node) => DetectTempTableDdl(node.OnName);

            private static bool IsTempTable(SchemaObjectName tableName)
            {
                return tableName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix);
            }

            private void DetectTempTableDdl(SchemaObjectName tableName)
            {
                if (IsTempTable(tableName))
                {
                    Callback(tableName, "DDL");
                }
            }

            private void RegisterTempTable(Identifier tableName)
            {
                // temp table name reuse is not allowed so no check for existence in the collection
                TempTables.Add(tableName.Value);
            }

            private void RegisterTempTable(SchemaObjectName tableName)
            {
                if (!TempTables.Add(tableName.BaseIdentifier.Value))
                {
                    // TODO : move message to resources, add translations
                    // name reused
                    Callback(tableName, "name reused");
                }
            }
        }
    }
}
