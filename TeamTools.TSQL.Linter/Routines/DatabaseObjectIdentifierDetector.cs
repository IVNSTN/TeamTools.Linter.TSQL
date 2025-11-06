using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class DatabaseObjectIdentifierDetector : TSqlFragmentVisitor
    {
        private static readonly char[] TrimmedChars = new char[] { '@' };
        private readonly Action<Identifier, string> callback;
        private readonly Action<SchemaObjectName> schemaObjectCallback;
        private readonly bool definitionOnly = false;
        private readonly bool ignoreVariables = false;
        private readonly bool ignoreAliases = false;

        public DatabaseObjectIdentifierDetector(Action<Identifier, string> callback, bool definitionOnly)
        {
            this.callback = callback;
            this.definitionOnly = definitionOnly;
        }

        public DatabaseObjectIdentifierDetector(Action<Identifier, string> callback, bool definitionOnly, bool ignoreVariables, bool ignoreAliases)
        {
            this.callback = callback;
            this.definitionOnly = definitionOnly;
            this.ignoreVariables = ignoreVariables;
            this.ignoreAliases = ignoreAliases;
        }

        public DatabaseObjectIdentifierDetector(Action<SchemaObjectName> callback, bool definitionOnly)
        {
            schemaObjectCallback = callback;
            this.definitionOnly = definitionOnly;
        }

        public DatabaseObjectIdentifierDetector(
            Action<Identifier, string> callback,
            Action<SchemaObjectName> schemaCallback,
            bool definitionOnly,
            bool ignoreVariables,
            bool ignoreAliases)
        {
            this.callback = callback;
            schemaObjectCallback = schemaCallback;
            this.definitionOnly = definitionOnly;
            this.ignoreVariables = ignoreVariables;
            this.ignoreAliases = ignoreAliases;
        }

        public DatabaseObjectIdentifierDetector(Action<Identifier, string> callback)
        {
            this.callback = callback;
        }

        public override void Visit(SchemaObjectName node)
        {
            // not all the references
            if (definitionOnly)
            {
                return;
            }

            SchemaIdentifierDetected(node);
            IdentifierDetected(node.SchemaIdentifier);
            IdentifierDetected(node.BaseIdentifier);
        }

        public override void Visit(DropObjectsStatement node)
        {
            // not all the references
            if (definitionOnly)
            {
                return;
            }

            foreach (var obj in node.Objects)
            {
                SchemaIdentifierDetected(obj);
                IdentifierDetected(obj.SchemaIdentifier);
                IdentifierDetected(obj.BaseIdentifier);
            }
        }

        public override void Visit(TableReferenceWithAliasAndColumns node)
        {
            // Generic SchemaObjectName would catch
            if (definitionOnly)
            {
                return;
            }

            foreach (var col in node.Columns)
            {
                IdentifierDetected(col);
            }
        }

        public override void Visit(CreateTableStatement node)
        {
            // Generic SchemaObjectName would catch
            if (!definitionOnly)
            {
                return;
            }

            SchemaIdentifierDetected(node.SchemaObjectName);
            IdentifierDetected(node.SchemaObjectName.BaseIdentifier);
        }

        public override void Visit(ProcedureStatementBody node)
        {
            // Generic SchemaObjectName would catch
            if (!definitionOnly)
            {
                return;
            }

            SchemaIdentifierDetected(node.ProcedureReference.Name);
            IdentifierDetected(node.ProcedureReference.Name?.BaseIdentifier);
        }

        public override void Visit(ViewStatementBody node)
        {
            // Generic SchemaObjectName would catch
            if (!definitionOnly)
            {
                return;
            }

            SchemaIdentifierDetected(node.SchemaObjectName);
            IdentifierDetected(node.SchemaObjectName.BaseIdentifier);
        }

        public override void Visit(TriggerStatementBody node)
        {
            // Generic SchemaObjectName would catch
            if (!definitionOnly)
            {
                return;
            }

            // DDL-triggers dont have schema
            if (node.TriggerObject.TriggerScope == TriggerScope.Normal)
            {
                SchemaIdentifierDetected(node.Name);
            }

            IdentifierDetected(node.Name.BaseIdentifier);
        }

        public override void Visit(FunctionStatementBody node)
        {
            // Generic SchemaObjectName would catch
            if (!definitionOnly)
            {
                return;
            }

            SchemaIdentifierDetected(node.Name);
            IdentifierDetected(node.Name.BaseIdentifier);
        }

        public override void Visit(CreateTypeStatement node)
        {
            // Generic SchemaObjectName would catch
            if (!definitionOnly)
            {
                return;
            }

            SchemaIdentifierDetected(node.Name);
            IdentifierDetected(node.Name.BaseIdentifier);
        }

        public override void Visit(CreateTypeTableStatement node)
        {
            // Generic SchemaObjectName would catch
            if (!definitionOnly)
            {
                return;
            }

            SchemaIdentifierDetected(node.Name);
            IdentifierDetected(node.Name.BaseIdentifier);
        }

        public override void Visit(DeclareVariableElement node)
        {
            if (ignoreVariables)
            {
                return;
            }

            IdentifierDetected(node.VariableName, node.VariableName.Value.TrimStart(TrimmedChars));
        }

        public override void Visit(DeclareTableVariableBody node)
        {
            if (ignoreVariables)
            {
                return;
            }

            if (node.VariableName == null)
            {
                // in inline-table function output definition has no name
                return;
            }

            IdentifierDetected(node.VariableName, node.VariableName.Value.TrimStart(TrimmedChars));
        }

        public override void Visit(DeclareCursorStatement node) => IdentifierDetected(node.Name, node.Name.Value.TrimStart(TrimmedChars));

        public override void Visit(ConstraintDefinition node) => IdentifierDetected(node.ConstraintIdentifier);

        public override void Visit(CreateSynonymStatement node) => SchemaIdentifierDetected(node.Name);

        public override void Visit(IndexDefinition node) => IdentifierDetected(node.Name);

        public override void Visit(IndexStatement node) => IdentifierDetected(node.Name);

        public override void Visit(ColumnDefinition node) => IdentifierDetected(node.ColumnIdentifier);

        public override void Visit(CreateSchemaStatement node) => IdentifierDetected(node.Name);

        public override void Visit(CreateServiceStatement node) => IdentifierDetected(node.Name);

        public override void Visit(CreateMessageTypeStatement node) => IdentifierDetected(node.Name);

        public override void Visit(CreateRoleStatement node) => IdentifierDetected(node.Name);

        public override void Visit(CreatePartitionSchemeStatement node) => IdentifierDetected(node.Name);

        public override void Visit(TableReferenceWithAlias node)
        {
            if (ignoreAliases)
            {
                return;
            }

            IdentifierDetected(node.Alias);
        }

        public override void Visit(SelectScalarExpression node)
        {
            if (definitionOnly || ignoreAliases)
            {
                return;
            }

            IdentifierDetected(node.ColumnName?.Identifier);
        }

        public override void Visit(CommonTableExpression node)
        {
            if (ignoreAliases)
            {
                return;
            }

            IdentifierDetected(node.ExpressionName);
        }

        public override void Visit(SecurityStatement node)
        {
            if (node.SecurityTargetObject == null || node.SecurityTargetObject.ObjectKind == SecurityObjectKind.NotSpecified)
            {
                return;
            }

            foreach (var id in node.SecurityTargetObject.ObjectName.MultiPartIdentifier.Identifiers)
            {
                IdentifierDetected(id);
            }
        }

        private void IdentifierDetected(Identifier node, string cleanedName = null)
        {
            if (node is null)
            {
                return;
            }

            if (string.IsNullOrEmpty(cleanedName))
            {
                cleanedName = node.Value;
            }

            callback?.Invoke(node, cleanedName);
        }

        private void SchemaIdentifierDetected(SchemaObjectName node)
        {
            schemaObjectCallback?.Invoke(node);
        }
    }
}
