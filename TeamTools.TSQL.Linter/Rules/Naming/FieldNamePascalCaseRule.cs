using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0967", "FIELD_NAME_PASCAL_CASE")]
    internal sealed class FieldNamePascalCaseRule : BaseNamingConventionRule
    {
        private static readonly NamingNotationKind ExpectedNotation = NamingNotationKind.PascalCase;

        public FieldNamePascalCaseRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node)
        {
            ValidateNamingNotation(node, node.ColumnIdentifier.Value, ExpectedNotation);
        }
    }
}
