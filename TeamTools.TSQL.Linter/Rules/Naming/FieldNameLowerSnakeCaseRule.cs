using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0966", "FIELD_NAME_LOWER_SNAKE_CASE")]
    internal sealed class FieldNameLowerSnakeCaseRule : BaseNamingConventionRule
    {
        private static readonly NamingNotationKind ExpectedNotation = NamingNotationKind.SnakeLowerCase;

        public FieldNameLowerSnakeCaseRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node)
        {
            ValidateNamingNotation(node, node.ColumnIdentifier.Value, ExpectedNotation);
        }
    }
}
