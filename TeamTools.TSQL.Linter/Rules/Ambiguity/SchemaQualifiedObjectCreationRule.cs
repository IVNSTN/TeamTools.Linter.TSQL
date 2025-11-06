using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0108", "CREATE_WITH_SCHEMA")]
    internal sealed class SchemaQualifiedObjectCreationRule : AbstractRule
    {
        public SchemaQualifiedObjectCreationRule() : base()
        {
        }

        public override void Visit(CreateProcedureStatement node) => ValidateSchema(node.ProcedureReference.Name, true);

        public override void Visit(CreateFunctionStatement node) => ValidateSchema(node.Name);

        public override void Visit(CreateTypeStatement node) => ValidateSchema(node.Name);

        public override void Visit(CreateQueueStatement node) => ValidateSchema(node.Name);

        public override void Visit(CreateViewStatement node) => ValidateSchema(node.SchemaObjectName);

        public override void Visit(CreateTableStatement node) => ValidateSchema(node.SchemaObjectName, true);

        public override void Visit(CreateTriggerStatement node)
        {
            if (node.TriggerObject.TriggerScope != TriggerScope.Normal)
            {
                // DDL trigger cannot have schema
                return;
            }

            ValidateSchema(node.Name);
        }

        public override void Visit(CreateSynonymStatement node)
        {
            ValidateSchema(node.Name);
            ValidateSchema(node.ForName);
        }

        private static bool CheckSchemaIdentifier(Identifier schemaIdentifier)
            => !(schemaIdentifier is null || string.IsNullOrEmpty(schemaIdentifier.Value));

        private void ValidateSchema(SchemaObjectName name, bool ignoreTempObjects = false)
        {
            if (ignoreTempObjects && name.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                // temp objects should not require schema since their scope is limited
                return;
            }

            if (!CheckSchemaIdentifier(name.SchemaIdentifier))
            {
                HandleNodeError(name);
            }
        }
    }
}
