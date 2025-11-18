using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Instruction detector.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal partial class NativelyUnsupportedInstructionRule
    {
        private class NativelyUnsupportedInstructionDetector : TSqlFragmentVisitor
        {
            private static readonly Dictionary<BinaryQueryExpressionType, string> UnsupportedBinaryQuery = new Dictionary<BinaryQueryExpressionType, string>
            {
                { BinaryQueryExpressionType.Intersect, "INTERSECT" },
                { BinaryQueryExpressionType.Except, "EXCEPT" },
            };

            public NativelyUnsupportedInstructionDetector(Action<TSqlFragment, string> callback)
            {
                Callback = callback;
            }

            private Action<TSqlFragment, string> Callback { get; }

            public override void Visit(UpdateSpecification node)
            {
                if (node.FromClause != null)
                {
                    Callback(node.FromClause, "FROM in UPDATE");
                }
            }

            public override void Visit(SetTransactionIsolationLevelStatement node)
            {
                if (node.Level == IsolationLevel.ReadCommitted
                || node.Level == IsolationLevel.ReadUncommitted)
                {
                    Callback(node, "READ (UN)COMMITTED isolation level");
                }
            }

            public override void Visit(OpenRowsetTableReference node) => Callback(node, "OPENROWSET");

            public override void Visit(OpenQueryTableReference node) => Callback(node, "OPENQUERY");

            public override void Visit(ExternalDataSourceStatement node) => Callback(node, "OPENDATASOURCE");

            public override void Visit(OpenXmlTableReference node) => Callback(node, "OPENXML");

            public override void Visit(FullTextPredicate node) => Callback(node, "fulltext search");

            public override void Visit(GoToStatement node) => Callback(node, "GOTO");

            public override void Visit(TableSampleClause node) => Callback(node, "TABLESAMPLE");

            public override void Visit(PivotedTableReference node) => Callback(node, "PIVOT");

            public override void Visit(UnpivotedTableReference node) => Callback(node, "UNPIVOT");

            public override void Visit(TransactionStatement node) => Callback(node, "tran control");

            public override void Visit(PrintStatement node) => Callback(node, "PRINT");

            public override void Visit(RaiseErrorStatement node) => Callback(node, "RAISERROR");

            public override void Visit(RollupGroupingSpecification node) => Callback(node, "ROLLUP");

            public override void Visit(CubeGroupingSpecification node) => Callback(node, "CUBE");

            public override void Visit(XmlForClause node) => Callback(node, "FOR XML");

            public override void Visit(BrowseForClause node) => Callback(node, "FOR BROWSE");

            public override void Visit(GeneralSetCommand node) => Callback(node, "SET options");

            public override void Visit(SetOnOffStatement node) => Callback(node, "SET options");

            public override void Visit(StatisticsOption node) => Callback(node, "SET STATISTICS");

            public override void Visit(ReceiveStatement node) => Callback(node, "RECEIVE from queue");

            public override void Visit(SendStatement node) => Callback(node, "SEND to queue");

            public override void Visit(DbccStatement node) => Callback(node, "DBCC");

            public override void Visit(CreateDatabaseStatement node) => Callback(node, "CREATE DB");

            public override void Visit(DropDatabaseStatement node) => Callback(node, "DROP DB");

            public override void Visit(AlterDatabaseStatement node) => Callback(node, "ALTER DB");

            public override void Visit(TruncateTableStatement node) => Callback(node, "TRUNCATE");

            public override void Visit(ExecuteInsertSource node) => Callback(node, "INSERT-EXEC");

            public override void Visit(CreateTableStatement node) => Callback(node, "DDL CREATE");

            public override void Visit(AlterTableStatement node) => Callback(node, "DDL ALTER");

            public override void Visit(DropObjectsStatement node) => Callback(node, "DDL DROP");

            public override void Visit(CommonTableExpression node) => Callback(node, "CTE");

            public override void Visit(DeclareTableVariableBody node) => Callback(node, "table variable");

            public override void Visit(DeclareCursorStatement node) => Callback(node, "CURSOR");

            public override void Visit(FetchCursorStatement node) => Callback(node, "CURSOR");

            public override void Visit(CursorStatement node) => Callback(node, "CURSOR");

            public override void Visit(OverClause node) => Callback(node, "window function");

            public override void Visit(ValuesInsertSource node)
            {
                if (node.RowValues.Count > 1)
                {
                    Callback(node.RowValues[1], "multiple VALUES rows");
                }
            }

            public override void Visit(WhereClause node)
            {
                if (node.Cursor != null)
                {
                    Callback(node, "CURSOR");
                }
            }

            public override void Visit(NamedTableReference node)
            {
                if (node.SchemaObject.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
                {
                    Callback(node, "temp table ref");
                }
            }

            public override void Visit(SelectStatement node)
            {
                if (node.Into != null)
                {
                    Callback(node, "SELECT INTO");
                }
            }

            public override void Visit(QualifiedJoin node)
            {
                if (node.JoinHint == JoinHint.None)
                {
                    return;
                }

                if (node.JoinHint == JoinHint.Hash)
                {
                    Callback(node, "HASH JOIN");
                }
                else if (node.JoinHint == JoinHint.Merge)
                {
                    Callback(node, "MERGE JOIN");
                }
            }

            public override void Visit(TopRowFilter node)
            {
                if (node.Percent)
                {
                    Callback(node, "TOP PERCENT");
                }
            }

            public override void Visit(BinaryQueryExpression node)
            {
                if (UnsupportedBinaryQuery.TryGetValue(node.BinaryQueryExpressionType, out var queryType))
                {
                    Callback(node, queryType);
                }
            }
        }
    }
}
