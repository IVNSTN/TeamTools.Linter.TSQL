using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0974", "VARIABLE_NAME_LOWER_SNAKE_CASE")]
    internal sealed class VariableNameLowerSnakeCaseRule : BaseNamingConventionRule
    {
        private static readonly NamingNotationKind ExpectedNotation = NamingNotationKind.SnakeLowerCase;

        public VariableNameLowerSnakeCaseRule() : base()
        {
        }

        // table variables are handled by table naming convention rule
        public override void Visit(DeclareVariableElement node)
        {
            ValidateNamingNotation(node, node.VariableName.Value, ExpectedNotation);
        }
    }
}
