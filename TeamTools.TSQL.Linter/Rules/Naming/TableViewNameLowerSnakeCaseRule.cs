using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0963", "TABLE_NAME_LOWER_SNAKE_CASE")]
    internal sealed class TableViewNameLowerSnakeCaseRule : BaseNamingConventionRule
    {
        private static readonly NamingNotationKind ExpectedNotation = NamingNotationKind.SnakeLowerCase;

        public TableViewNameLowerSnakeCaseRule() : base()
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
