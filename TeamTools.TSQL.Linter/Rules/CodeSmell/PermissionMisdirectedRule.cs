using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0164", "PERMISSION_MISDIRECTED")]
    [SecurityRule]
    internal sealed class PermissionMisdirectedRule : AbstractRule
    {
        public PermissionMisdirectedRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var mainObject = new MainScriptObjectDetector();
            node.Accept(mainObject);
            if (string.IsNullOrWhiteSpace(mainObject.ObjectFullName))
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
                    if (target == null)
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

            foreach (var batch in node.Batches)
            {
                // TODO : looks like crutch
                if (batch.FirstTokenIndex == mainObject.ObjectDefinitionBatch.FirstTokenIndex)
                {
                    continue;
                }

                batch.AcceptChildren(grantVisitor);
            }
        }
    }
}
