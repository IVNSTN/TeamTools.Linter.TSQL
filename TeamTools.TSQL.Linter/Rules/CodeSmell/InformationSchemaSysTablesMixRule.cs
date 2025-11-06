using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0781", "INFORMATION_SCHEMA_SYS_MIX")]
    internal sealed class InformationSchemaSysTablesMixRule : AbstractRule
    {
        public InformationSchemaSysTablesMixRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            var schemaFinder = new SchemaFinder();
            node.Accept(schemaFinder);

            if (schemaFinder.InformationSchemaReference != null && schemaFinder.SysSchemaReference != null)
            {
                HandleNodeError(schemaFinder.InformationSchemaReference);
            }
        }

        private class SchemaFinder : TSqlFragmentVisitor
        {
            public TSqlFragment InformationSchemaReference { get; private set; }

            public TSqlFragment SysSchemaReference { get; private set; }

            public override void Visit(NamedTableReference node)
            {
                string schemaName = node.SchemaObject.SchemaIdentifier?.Value ?? "";
                string objectName = node.SchemaObject.BaseIdentifier.Value;

                if (schemaName.Equals("INFORMATION_SCHEMA", StringComparison.OrdinalIgnoreCase))
                {
                    InformationSchemaReference = node;
                }
                else if (schemaName.Equals("sys", StringComparison.OrdinalIgnoreCase))
                {
                    SysSchemaReference = node;
                }
                else if (string.IsNullOrEmpty(schemaName)
                && objectName.StartsWith("sys", StringComparison.OrdinalIgnoreCase))
                {
                    SysSchemaReference = node;
                }
            }
        }
    }
}
