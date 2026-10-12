using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0740", "ALTER_DROP_NEEDS_SCHEMA")]
    internal sealed class SchemaQualifiedAlterDropRule : AbstractRule
    {
        private readonly ObjectSchemaMandatorityValidator schemaValidator;

        public SchemaQualifiedAlterDropRule() : base()
        {
            schemaValidator = new ObjectSchemaMandatorityValidator(ViolationHandler);
        }

        public override void Visit(AlterTableStatement node) => ValidateSchema(node.SchemaObjectName, true);

        public override void Visit(DropTableStatement node) => ValidateSchema(node.Objects, true);

        public override void Visit(CreateOrAlterProcedureStatement node) => ValidateSchema(node.ProcedureReference.Name);

        public override void Visit(AlterProcedureStatement node) => ValidateSchema(node.ProcedureReference.Name);

        public override void Visit(DropProcedureStatement node) => ValidateSchema(node.Objects);

        public override void Visit(CreateOrAlterFunctionStatement node) => ValidateSchema(node.Name);

        public override void Visit(AlterFunctionStatement node) => ValidateSchema(node.Name);

        public override void Visit(DropFunctionStatement node) => ValidateSchema(node.Objects);

        public override void Visit(CreateOrAlterTriggerStatement node) => ValidateSchema(node.Name);

        public override void Visit(CreateOrAlterViewStatement node) => ValidateSchema(node.SchemaObjectName);

        public override void Visit(AlterViewStatement node) => ValidateSchema(node.SchemaObjectName);

        public override void Visit(DropViewStatement node) => ValidateSchema(node.Objects);

        public override void Visit(DropTypeStatement node) => ValidateSchema(node.Name);

        public override void Visit(AlterQueueStatement node) => ValidateSchema(node.Name);

        public override void Visit(DropQueueStatement node) => ValidateSchema(node.Name);

        public override void Visit(DropSynonymStatement node) => ValidateSchema(node.Objects);

        public override void Visit(AlterSequenceStatement node) => ValidateSchema(node.Name);

        public override void Visit(DropSequenceStatement node) => ValidateSchema(node.Objects);

        public override void Visit(AlterTriggerStatement node)
        {
            if (node.TriggerObject.TriggerScope != TriggerScope.Normal)
            {
                // DDL trigger cannot have schema
                return;
            }

            ValidateSchema(node.Name);
        }

        public override void Visit(DropTriggerStatement node)
        {
            if (node.TriggerScope != TriggerScope.Normal)
            {
                // DDL trigger cannot have schema
                return;
            }

            ValidateSchema(node.Objects);
        }

        private void ValidateSchema(SchemaObjectName node, bool ignoreTempObjects = false)
            => schemaValidator.ValidateSchema(node, ignoreTempObjects);

        private void ValidateSchema(IList<SchemaObjectName> objects, bool ignoreTempObjects = false)
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                ValidateSchema(objects[i], ignoreTempObjects);
            }
        }
    }
}
