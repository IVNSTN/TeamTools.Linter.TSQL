using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0109", "TABLE_REF_SCHEMA")]
    internal sealed class SchemaQualifiedTableReferenceRule : AbstractRule
    {
        public SchemaQualifiedTableReferenceRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            var aliases = new AliasVisitor();
            node.Accept(aliases);
            var tableRefs = new TableReferenceVisitor(aliases.DetectedAliases, ViolationHandlerWithMessage);
            node.Accept(tableRefs);
        }

        private class TableReferenceVisitor : TSqlFragmentVisitor
        {
            private readonly HashSet<string> aliases;
            private readonly Action<TSqlFragment, string> callback;

            public TableReferenceVisitor(HashSet<string> aliases, Action<TSqlFragment, string> callback)
            {
                this.aliases = aliases;
                this.callback = callback;
            }

            public override void Visit(CreateTableStatement node) => ValidateIdentifier(node.SchemaObjectName);

            public override void Visit(DropTableStatement node) => ValidateIdentifierList(node.Objects);

            public override void Visit(AlterTableStatement node) => ValidateIdentifier(node.SchemaObjectName);

            public override void Visit(TruncateTableStatement node) => ValidateIdentifier(node.TableName);

            public override void Visit(CreateIndexStatement node) => ValidateIdentifier(node.OnName);

            public override void Visit(CreateStatisticsStatement node) => ValidateIdentifier(node.OnName);

            public override void Visit(NamedTableReference node) => ValidateIdentifier(node.SchemaObject, true);

            public override void Visit(CreateTriggerStatement node)
            {
                if (node.TriggerObject.TriggerScope != TriggerScope.Normal)
                {
                    // db-level triggers do not have schema
                    return;
                }

                ValidateIdentifier(node.TriggerObject.Name);
            }

            private static bool CheckSchemaIdentifier(Identifier schemaIdentifier)
                => !string.IsNullOrEmpty(schemaIdentifier?.Value);

            private static bool IsSupportedIdentifier(Identifier table)
            {
                return !(table.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix)
                    || TSqlDomainAttributes.IsTriggerSystemTable(table.Value));
            }

            private void ValidateIdentifier(SchemaObjectName identifier, bool possibleAlias = false)
            {
                if (!IsSupportedIdentifier(identifier.BaseIdentifier))
                {
                    return;
                }

                if (CheckSchemaIdentifier(identifier.SchemaIdentifier))
                {
                    return;
                }

                if (possibleAlias && aliases.Contains(identifier.BaseIdentifier.Value))
                {
                    return;
                }

                callback(identifier.BaseIdentifier, identifier.BaseIdentifier.Value);
            }

            private void ValidateIdentifierList(IList<SchemaObjectName> identifiers)
            {
                int n = identifiers.Count;
                for (int i = 0; i < n; i++)
                {
                    ValidateIdentifier(identifiers[i]);
                }
            }
        }

        private sealed class AliasVisitor : TSqlFragmentVisitor
        {
            public HashSet<string> DetectedAliases { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public override void Visit(TableReferenceWithAlias node) => RegisterAlias(node.Alias);

            public override void Visit(CommonTableExpression node) => RegisterAlias(node.ExpressionName);

            private void RegisterAlias(Identifier alias)
            {
                if (alias != null)
                {
                    DetectedAliases.Add(alias.Value);
                }
            }
        }
    }
}
