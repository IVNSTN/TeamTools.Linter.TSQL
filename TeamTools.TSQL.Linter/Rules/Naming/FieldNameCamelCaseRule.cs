using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0968", "FIELD_NAME_CAMEL_CASE")]
    internal sealed class FieldNameCamelCaseRule : BaseNamingConventionRule
    {
        private static readonly NamingNotationKind ExpectedNotation = NamingNotationKind.CamelCase;

        public FieldNameCamelCaseRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node)
        {
            ValidateNamingNotation(node, node.ColumnIdentifier.Value, ExpectedNotation);
        }
    }
}
