using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines
{
    internal sealed class GrantTargetVisitor : TSqlFragmentVisitor
    {
        private readonly Action<SecurityStatement, string> callbackForTarget;
        private readonly Action<SecurityStatement, string> callbackForPrincipal;

        public GrantTargetVisitor(Action<SecurityStatement, string> targetDetected, Action<SecurityStatement, string> principalDetected)
        {
            callbackForTarget = targetDetected;
            callbackForPrincipal = principalDetected;
        }

        public override void Visit(SecurityStatement node)
        {
            string target = node.SecurityTargetObject?.ObjectName.MultiPartIdentifier.Identifiers.
                Select(i => i.Value).Aggregate((current, next) => current + TSqlDomainAttributes.NamePartSeparator + next);

            TargetDetected(node, target);

            foreach (var p in node.Principals)
            {
                PrincipalDetected(node, p.Identifier != null ? p.Identifier.Value : p.PrincipalType.ToString());
            }
        }

        private void TargetDetected(SecurityStatement node, string name)
        {
            callbackForTarget?.Invoke(node, name);
        }

        private void PrincipalDetected(SecurityStatement node, string name)
        {
            callbackForPrincipal?.Invoke(node, name);
        }
    }
}
