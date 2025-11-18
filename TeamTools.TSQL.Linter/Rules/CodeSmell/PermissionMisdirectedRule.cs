using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0164", "PERMISSION_MISDIRECTED")]
    [SecurityRule]
    internal sealed class PermissionMisdirectedRule : ScriptAnalysisServiceConsumingRule
    {
        public PermissionMisdirectedRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var mainObject = GetService<MainScriptObjectDetector>(node);
            if (string.IsNullOrWhiteSpace(mainObject?.ObjectFullName))
            {
                return;
            }

            Action<SecurityStatement, string> grantTargetCallback = null;
            Action<SecurityStatement, string> grantPrincipalCallback = null;

            if ((mainObject.ObjectDefinitionNode is CreateRoleStatement)
                || (mainObject.ObjectDefinitionNode is CreateApplicationRoleStatement)
                || (mainObject.ObjectDefinitionNode is CreateUserStatement))
            {
                grantPrincipalCallback =
                    (nd, principal) =>
                    {
                        if (principal.Equals(mainObject.ObjectFullName, StringComparison.OrdinalIgnoreCase))
                        {
                            return;
                        }

                        HandleNodeError(nd);
                    };
            }
            else
            {
                grantTargetCallback = (nd, target) =>
                {
                    if (target is null)
                    {
                        HandleNodeError(nd);
                        return;
                    }

                    if (mainObject.ObjectFullName.Contains(TSqlDomainAttributes.NamePartSeparator)
                        && !target.Contains(TSqlDomainAttributes.NamePartSeparator))
                    {
                        target = string.Concat(TSqlDomainAttributes.DefaultSchemaPrefix, target);
                    }

                    if (target.Equals(mainObject.ObjectFullName, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    HandleNodeError(nd);
                };
            }

            var grantVisitor = new GrantTargetVisitor(grantTargetCallback, grantPrincipalCallback);

            int n = node.Batches.Count;
            for (int i = 0; i < n; i++)
            {
                var batch = node.Batches[i];

                // There is nothing to check in CREATE PROC/TABLE batch
                if (batch.FirstTokenIndex == mainObject.ObjectDefinitionBatch.FirstTokenIndex)
                {
                    continue;
                }

                batch.AcceptChildren(grantVisitor);
            }
        }
    }
}
