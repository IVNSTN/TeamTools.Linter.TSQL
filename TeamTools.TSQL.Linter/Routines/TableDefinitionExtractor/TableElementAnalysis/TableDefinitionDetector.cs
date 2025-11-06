using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor
{
    internal class TableDefinitionDetector : TSqlFragmentVisitor
    {
        private readonly Action<string, SqlTableDefinition> callback;
        private readonly Action<string, IList<ColumnDefinition>> columnCallback;

        public TableDefinitionDetector(
            Action<string, SqlTableDefinition> callback,
            Action<string, IList<ColumnDefinition>> columnCallback)
        {
            this.callback = callback;
            this.columnCallback = columnCallback;
        }

        public override void Visit(CreateTableStatement node)
        {
            if (node.Definition is null)
            {
                // filestream
                return;
            }

            string name = node.SchemaObjectName.GetFullName();
            var tableType = name.StartsWith(TSqlDomainAttributes.TempTablePrefix) ? SqlTableType.TempTable : SqlTableType.Default;

            callback(name, new SqlTableDefinition(node.Definition, tableType));
        }

        public override void Visit(DeclareTableVariableBody node)
        {
            if (node.VariableName is null)
            {
                // table-valued function return type definition
                return;
            }

            callback(node.VariableName.Value, new SqlTableDefinition(node.Definition, SqlTableType.TableVariable));
        }

        public override void Visit(CreateTypeTableStatement node)
        {
            callback(node.Name.GetFullName(), new SqlTableDefinition(node.Definition, SqlTableType.TypeTable));
        }

        // Not sure if this should be supported
        public override void Visit(AlterTableAddTableElementStatement node)
        {
            columnCallback(
                node.SchemaObjectName.GetFullName(),
                node.Definition.ColumnDefinitions);
        }
    }
}
