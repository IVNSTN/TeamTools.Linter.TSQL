using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0973", "VARIABLE_NAME_PASCAL_CASE")]
    internal sealed class VariableNamePascalCaseRule : BaseNamingConventionRule
    {
        private static readonly NamingNotationKind ExpectedNotation = NamingNotationKind.PascalCase;

        public VariableNamePascalCaseRule() : base()
        {
        }

        // table variables are handled by table naming convention rule
        public override void Visit(DeclareVariableElement node)
        {
            ValidateNamingNotation(node, node.VariableName.Value, ExpectedNotation);
        }
    }
}
