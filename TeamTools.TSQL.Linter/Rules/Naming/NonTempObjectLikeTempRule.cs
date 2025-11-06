using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics.CodeAnalysis;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0712", "NON_TEMP_OBJECT_LIKE_TEMP")]
    internal sealed class NonTempObjectLikeTempRule : AbstractRule
    {
        public NonTempObjectLikeTempRule() : base()
        {
        }

        public override void Visit(TriggerStatementBody node) => ValidateObjectName(node.Name, "TRIGGER");

        [ExcludeFromCodeCoverage]
        // Current parser version fails on temp function create attempt
        public override void Visit(FunctionStatementBody node) => ValidateObjectName(node.Name, "FUNCTION");

        [ExcludeFromCodeCoverage]
        // Current parser version fails on temp view create attempt
        public override void Visit(ViewStatementBody node) => ValidateObjectName(node.SchemaObjectName, "VIEW");

        // It covers CreateTypeTableStatement
        public override void Visit(CreateTypeStatement node) => ValidateObjectName(node.Name, "TYPE");

        public override void Visit(CreateSchemaStatement node) => ValidateObjectName(node.Name, "SCHEMA");

        public override void Visit(CreateSynonymStatement node) => ValidateObjectName(node.Name, "SYNONYM");

        public override void Visit(CreateServiceStatement node) => ValidateObjectName(node.Name, "SERVICE");

        public override void Visit(CreateMessageTypeStatement node) => ValidateObjectName(node.Name, "MESSAGE TYPE");

        private void ValidateObjectName(SchemaObjectName fullName, string objectType) => ValidateObjectName(fullName.BaseIdentifier, objectType);

        private void ValidateObjectName(Identifier name, string objectType)
        {
            if (name.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                HandleNodeError(name, objectType);
            }
        }
    }
}
