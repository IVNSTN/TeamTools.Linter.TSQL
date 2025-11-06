using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0971", "SB_NAME_LOWER_SNAKE_CASE")]
    internal sealed class ServiceBrokerPublicNameLowerSnakeCaseRule : BaseNamingConventionRule
    {
        private static readonly NamingNotationKind ExpectedNotation = NamingNotationKind.SnakeLowerCase;

        public ServiceBrokerPublicNameLowerSnakeCaseRule() : base()
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
