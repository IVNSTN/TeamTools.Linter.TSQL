using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal sealed class GrantTargetVisitor : TSqlFragmentVisitor
    {
        private static readonly Dictionary<PrincipalType, string> PrincipalTypes = new Dictionary<PrincipalType, string>
        {
            { PrincipalType.Identifier, "Identifier" },
            { PrincipalType.Null, "NULL" },
            { PrincipalType.Public, "PUBLIC" },
        };

        private readonly Action<SecurityStatement, string> callbackForTarget;
        private readonly Action<SecurityStatement, string> callbackForPrincipal;

        public GrantTargetVisitor(Action<SecurityStatement, string> targetDetected, Action<SecurityStatement, string> principalDetected)
        {
            callbackForTarget = targetDetected;
            callbackForPrincipal = principalDetected;
        }

        public override void Visit(SecurityStatement node)
        {
            if (callbackForTarget != null)
            {
                string target = GetTargetName(node.SecurityTargetObject?.ObjectName.MultiPartIdentifier.Identifiers);
                // If target is null then this is some general permission like GRANT LOGIN
                callbackForTarget?.Invoke(node, target);
            }

            if (callbackForPrincipal != null)
            {
                int n = node.Principals.Count;
                for (int i = 0; i < n; i++)
                {
                    var p = node.Principals[i];
                    callbackForPrincipal(node, p.Identifier?.Value ?? PrincipalTypes[p.PrincipalType]);
                }
            }
        }

        private static string GetTargetName(IList<Identifier> nameParts)
        {
            if (nameParts is null || nameParts.Count == 0)
            {
                return default;
            }

            return nameParts.GetFullName(TSqlDomainAttributes.NamePartSeparator);
        }
    }
}
