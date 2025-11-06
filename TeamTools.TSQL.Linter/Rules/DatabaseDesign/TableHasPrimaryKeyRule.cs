using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0941", "TABLE_HAS_PK")]
    internal sealed class TableHasPrimaryKeyRule : AbstractRule
    {
        public TableHasPrimaryKeyRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var idxVisitor = new TablesAndIndicesVisitor();
            node.Accept(idxVisitor);

            ValidateIndices(idxVisitor);
        }

        private void ValidateIndices(TablesAndIndicesVisitor tableIndices)
        {
            foreach (var tbl in tableIndices.Tables)
            {
                if (!tableIndices.Indices.ContainsKey(tbl.Key)
                || !tableIndices.Indices[tbl.Key].Where(idx => idx.IsPrimaryKey || idx.IsColumnStore).Any())
                {
                    HandleNodeError(tbl.Value, tbl.Key);
                }
            }
        }

        private class TableConstraintInfo
        {
            public bool IsPrimaryKey { get; set; }

            public bool IsColumnStore { get; set; }
        }

        private class PrimaryKeyConstraintVisitor : TSqlFragmentVisitor
        {
            private readonly Action<UniqueConstraintDefinition> callback;

            public PrimaryKeyConstraintVisitor(Action<UniqueConstraintDefinition> callback)
            {
                this.callback = callback;
            }

            public override void Visit(UniqueConstraintDefinition node)
            {
                if (!node.IsPrimaryKey)
                {
                    return;
                }

                callback(node);
            }
        }

        private class TablesAndIndicesVisitor : TSqlFragmentVisitor
        {
            public IDictionary<string, TSqlFragment> Tables { get; } = new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

            public IDictionary<string, IList<TableConstraintInfo>> Indices { get; } = new Dictionary<string, IList<TableConstraintInfo>>(StringComparer.OrdinalIgnoreCase);

            public override void Visit(DeclareTableVariableBody node)
            {
                if (node.VariableName is null)
                {
                    // inline table function output definition
                    return;
                }

                string tblName = node.VariableName.Value;

                Tables.TryAdd(tblName, node.VariableName);

                ExtractConstraints(tblName, node.Definition);
            }

            public override void Visit(CreateTableStatement node)
            {
                string tblName = node.SchemaObjectName.GetFullName();

                Tables.TryAdd(tblName, node.SchemaObjectName);

                ExtractConstraints(tblName, node.Definition);
            }

            public override void Visit(AlterTableAddTableElementStatement node)
            {
                string tblName = node.SchemaObjectName.GetFullName();
                ExtractConstraints(tblName, node.Definition);
            }

            public override void Visit(CreateColumnStoreIndexStatement node)
            {
                string tblName = node.OnName.GetFullName();

                if (!Indices.ContainsKey(tblName))
                {
                    Indices.Add(tblName, new List<TableConstraintInfo>());
                }

                Indices[tblName].Add(new TableConstraintInfo { IsColumnStore = true });
            }

            private void ExtractConstraints(string tableName, TableDefinition tableDefinition)
            {
                if (tableDefinition is null)
                {
                    // e.g. FILETABLE
                    return;
                }

                tableDefinition.AcceptChildren(
                    new PrimaryKeyConstraintVisitor(pk =>
                    {
                        if (!Indices.ContainsKey(tableName))
                        {
                            Indices.Add(tableName, new List<TableConstraintInfo>());
                        }

                        Indices[tableName].Add(new TableConstraintInfo { IsPrimaryKey = true });
                    }));
            }
        }
    }
}
