using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Index name builder.
    /// </summary>
    internal partial class IndexNamePatternRule
    {
        public override void Visit(CreateTableStatement node)
        {
            ExtractTableRef(node.SchemaObjectName, out string tableSchema, out string tableName);
            ValidateInlineInidices(node, tableSchema, tableName);
        }

        public override void Visit(DeclareTableVariableBody node)
        {
            if (node.VariableName is null)
            {
                // Inline TVF return type definition has no table name
                return;
            }

            const string tableSchema = ""; // table variables don't have schema
            string tableName = node.VariableName.Value.TrimStart(TSqlDomainAttributes.VariablePrefixChar);

            ValidateInlineInidices(node, tableSchema, tableName);
        }

        public override void Visit(CreateIndexStatement node)
        {
            ExtractTableRef(node.OnName, out string tableSchema, out string tableName);
            string expectedName = IndexNameBuilder.Build(
                tableSchema,
                tableName,
                false,
                node.Clustered ?? false,
                node.Unique,
                node.FilterPredicate != null,
                node.IncludeColumns?.Count > 0,
                node.Columns.ExtractNames());

            ValidateIndexName(node.Name, expectedName);
        }

        public override void Visit(CreateColumnStoreIndexStatement node)
        {
            ExtractTableRef(node.OnName, out string tableSchema, out string tableName);
            string expectedName = IndexNameBuilder.Build(
                tableSchema,
                tableName,
                true,
                node.Clustered ?? false,
                false,
                node.FilterPredicate != null,
                false,
                node.Columns.ExtractNames());

            ValidateIndexName(node.Name, expectedName);
        }
    }
}
