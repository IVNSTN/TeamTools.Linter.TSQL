using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0964", "TABLE_NAME_UPPER_SNAKE_CASE")]
    internal sealed class TableViewNameUpperSnakeCaseRule : BaseNamingConventionRule
    {
        private static readonly NamingNotationKind ExpectedNotation = NamingNotationKind.SnakeUpperCase;

        public TableViewNameUpperSnakeCaseRule() : base()
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
