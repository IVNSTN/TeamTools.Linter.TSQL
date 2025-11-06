using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0969", "SB_NAME_URL")]
    internal sealed class ServiceBrokerPublicNameAsUrlRule : AbstractRule
    {
        public ServiceBrokerPublicNameAsUrlRule() : base()
        {
        }

        public override void Visit(CreateContractStatement node)
        {
            ValidateNamingNotation(node, node.Name.Value);
        }

        public override void Visit(CreateMessageTypeStatement node)
        {
            ValidateNamingNotation(node, node.Name.Value);
        }

        public override void Visit(CreateServiceStatement node)
        {
            ValidateNamingNotation(node, node.Name.Value);
        }

        private static bool IsValidUrl(string urlCandidate)
        {
            if (string.IsNullOrEmpty(urlCandidate))
            {
                return false;
            }

            if (!PathExtension.IsFullyQualified(urlCandidate))
            {
                return false;
            }

            try
            {
                var uri = new Uri(urlCandidate, UriKind.Absolute);
                return uri.IsAbsoluteUri
                    && !string.IsNullOrEmpty(uri.Host)
                    && !string.IsNullOrEmpty(uri.AbsolutePath);
            }
            catch
            {
                return false;
            }
        }

        private void ValidateNamingNotation(TSqlFragment node, string currentName)
        {
            if (IsValidUrl(currentName))
            {
                return;
            }

            HandleNodeError(node, currentName);
        }
    }
}
