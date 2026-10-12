using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Properties;
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
            private static readonly string MsgTableNameReused = Strings.ViolationDetails_TempTableCachingRule_TableNameReused;
            private static readonly string MsgNamedConstraintDefined = Strings.ViolationDetails_TempTableCachingRule_NamedConstraint;
            private static readonly string MsgDdlOnTableExecuted = Strings.ViolationDetails_TempTableCachingRule_DdlOnTable;

            private bool hasTableVariables;

            public TempTableVisitor(Action<TSqlFragment, string> callback)
            {
                Callback = callback;
            }

            public HashSet<string> TempTables { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public bool OperatesWithTempTables => TempTables.Count > 0 || hasTableVariables;

            private Action<TSqlFragment, string> Callback { get; }

            // Table variables are very similar to temp tables and can be cached as well
            public override void ExplicitVisit(DeclareTableVariableBody node)
            {
                // However name reuse is not allowed, DDL - not allowed. Checking constraints only.
                node.Definition.AcceptChildren(this);
                hasTableVariables = true;
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
                    // named constraint
                    Callback(node.ConstraintIdentifier, MsgNamedConstraintDefined);
                }
            }

            // Any DDL on a temp table prevents its caching
            public override void Visit(AlterTableStatement node) => DetectTempTableDdl(node.SchemaObjectName);

            public override void Visit(IndexStatement node) => DetectTempTableDdl(node.OnName);

            public override void Visit(CreateStatisticsStatement node) => DetectTempTableDdl(node.OnName);

            private static bool IsTempTable(SchemaObjectName tableName) => tableName.IsTempTable();

            private void DetectTempTableDdl(SchemaObjectName tableName)
            {
                if (IsTempTable(tableName))
                {
                    Callback(tableName, MsgDdlOnTableExecuted);
                }
            }

            private void RegisterTempTable(SchemaObjectName tableName)
            {
                if (!TempTables.Add(tableName.BaseIdentifier.Value))
                {
                    // name reused
                    Callback(tableName, MsgTableNameReused);
                }
            }
        }
    }
}
