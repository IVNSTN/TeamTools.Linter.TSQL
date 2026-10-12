using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0460", "NESTED_SAME_AS_TABLE")]
    internal sealed class SameNestedObjectNameRule : AbstractRule
    {
        public SameNestedObjectNameRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node) => ValidateNestedObjects(node, node.SchemaObjectName);

        public override void Visit(AlterTableStatement node) => ValidateNestedObjects(node, node.SchemaObjectName);

        // CREATE INDEX is a standalone statement which does not have
        // TableDefinition nested object with constraints and so on
        public override void Visit(CreateIndexStatement node)
        {
            string tableBaseName = node.OnName.BaseIdentifier.Value;

            if (IsIgnoredTable(tableBaseName))
            {
                return;
            }

            if (node.Name != null
                && string.Equals(node.Name.Value, tableBaseName, StringComparison.OrdinalIgnoreCase))
            {
                HandleNodeError(node.Name, $"INDEX {tableBaseName}");
            }
        }

        private static bool IsIgnoredTable(string baseName) =>
            baseName.StartsWith(TSqlDomainAttributes.TempTablePrefix, StringComparison.OrdinalIgnoreCase)
            || baseName.StartsWith(TSqlDomainAttributes.VariablePrefix, StringComparison.OrdinalIgnoreCase);

        private void ValidateNestedObjects(TSqlFragment node, SchemaObjectName tableName)
        {
            string baseName = tableName.BaseIdentifier.Value;

            if (IsIgnoredTable(baseName))
            {
                return;
            }

            var visitor = new NestedObjectVisitor(baseName, ViolationHandlerWithMessage);
            node.AcceptChildren(visitor);
        }

        private sealed class NestedObjectVisitor : TSqlFragmentVisitor
        {
            private readonly string tableBaseName;
            private readonly Action<TSqlFragment, string> callback;

            public NestedObjectVisitor(string tableBaseName, Action<TSqlFragment, string> callback)
                : base()
            {
                this.tableBaseName = tableBaseName;
                this.callback = callback;
            }

            public override void Visit(ColumnDefinition node) => ValidateName(node.ColumnIdentifier, "COLUMN");

            public override void Visit(ConstraintDefinition node) => ValidateName(node.ConstraintIdentifier, "CONSTRAINT");

            public override void Visit(IndexDefinition node) => ValidateName(node.Name, "INDEX");

            private void ValidateName(Identifier identifier, string nestedObjectType)
            {
                if (identifier is null || string.IsNullOrEmpty(identifier.Value))
                {
                    return;
                }

                if (string.Equals(identifier.Value, tableBaseName, StringComparison.OrdinalIgnoreCase))
                {
                    callback(identifier, $"{nestedObjectType} {tableBaseName}");
                }
            }
        }
    }
}
