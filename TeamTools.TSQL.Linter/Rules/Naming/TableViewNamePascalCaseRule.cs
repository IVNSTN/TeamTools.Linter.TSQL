using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0965", "TABLE_NAME_PASCAL_CASE")]
    internal sealed class TableViewNamePascalCaseRule : BaseNamingConventionRule
    {
        private static readonly NamingNotationKind ExpectedNotation = NamingNotationKind.PascalCase;

        public TableViewNamePascalCaseRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            ValidateNamingNotation(node, node.SchemaObjectName.BaseIdentifier.Value, ExpectedNotation);
        }

        public override void Visit(DeclareTableVariableBody node)
        {
            ValidateNamingNotation(node, node.VariableName?.Value, ExpectedNotation);
        }

        public override void Visit(CreateViewStatement node)
        {
            ValidateNamingNotation(node, node.SchemaObjectName.BaseIdentifier.Value, ExpectedNotation);
        }
    }
}
