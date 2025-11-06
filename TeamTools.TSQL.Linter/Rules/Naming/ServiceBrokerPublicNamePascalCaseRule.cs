using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0970", "SB_NAME_PASCAL_CASE")]
    internal sealed class ServiceBrokerPublicNamePascalCaseRule : BaseNamingConventionRule
    {
        private static readonly NamingNotationKind ExpectedNotation = NamingNotationKind.PascalCase;

        public ServiceBrokerPublicNamePascalCaseRule() : base()
        {
        }

        public override void Visit(CreateContractStatement node)
        {
            ValidateNamingNotation(node, node.Name.Value, ExpectedNotation);
        }

        public override void Visit(CreateMessageTypeStatement node)
        {
            ValidateNamingNotation(node, node.Name.Value, ExpectedNotation);
        }

        public override void Visit(CreateServiceStatement node)
        {
            ValidateNamingNotation(node, node.Name.Value, ExpectedNotation);
        }
    }
}
