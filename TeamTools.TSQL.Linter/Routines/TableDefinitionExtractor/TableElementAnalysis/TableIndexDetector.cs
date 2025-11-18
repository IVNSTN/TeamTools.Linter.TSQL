using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor
{
    internal class TableIndexDetector : TSqlFragmentVisitor
    {
        private readonly Action<SqlTableElement> callback;

        public TableIndexDetector(Action<SqlTableElement> callback)
        {
            this.callback = callback;
        }

        public override void Visit(CreateColumnStoreIndexStatement node)
        {
            string tableName = node.OnName.GetFullName();
            callback(SqlTableElementBuilder.Make(tableName, node));
        }

        public override void Visit(CreateIndexStatement node)
        {
            string tableName = node.OnName.GetFullName();
            callback(SqlTableElementBuilder.Make(tableName, node));
        }

        public override void Visit(CreateTableStatement node)
        {
            string tableName = node.SchemaObjectName.GetFullName();
            InlineIndexDefinitionDetector.DetectIndices(tableName, node, callback);
        }

        public override void Visit(DeclareTableVariableBody node)
        {
            if (node.VariableName is null)
            {
                // table-valued function return type definition
                return;
            }

            string tableName = node.VariableName.Value;
            InlineIndexDefinitionDetector.DetectIndices(tableName, node, callback);
        }

        public override void Visit(CreateTypeTableStatement node)
        {
            string tableName = node.Name.GetFullName();
            InlineIndexDefinitionDetector.DetectIndices(tableName, node, callback);
        }

        private sealed class InlineIndexDefinitionDetector : TSqlFragmentVisitor
        {
            private readonly string tableName;
            private readonly Action<SqlTableElement> callback;

            public InlineIndexDefinitionDetector(string tableName, Action<SqlTableElement> callback)
            {
                this.tableName = tableName;
                this.callback = callback;
            }

            public static void DetectIndices(string tableName, TSqlFragment node, Action<SqlTableElement> callback)
            {
                node.Accept(new InlineIndexDefinitionDetector(tableName, callback));
            }

            public override void Visit(IndexDefinition node)
            {
                callback?.Invoke(SqlTableElementBuilder.Make(tableName, node));
            }
        }
    }
}
