using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0972", "VARIABLE_NAME_CAMEL_CASE")]
    internal sealed class VariableNameCamelCaseRule : BaseNamingConventionRule
    {
        private static readonly NamingNotationKind ExpectedNotation = NamingNotationKind.CamelCase;

        public VariableNameCamelCaseRule() : base()
        {
        }

        // table variables are handled by table naming convention rule
        public override void Visit(DeclareVariableElement node)
        {
            ValidateNamingNotation(node, node.VariableName.Value, ExpectedNotation);
        }
    }
}
