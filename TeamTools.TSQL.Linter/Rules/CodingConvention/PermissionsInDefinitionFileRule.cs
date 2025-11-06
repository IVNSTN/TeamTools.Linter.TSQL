using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0165", "PERMISSION_IN_DEFINITION")]
    [SecurityRule]
    internal sealed class PermissionsInDefinitionFileRule : AbstractRule
    {
        // TODO : very similar to PermissionMisdirectedRule
        public PermissionsInDefinitionFileRule() : base()
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

            var grantVisitor = new GrantTargetVisitor(
                (nd, target) =>
                {
                    if (target == null)
                    {
                        return;
                    }

                    if (mainObject.ObjectFullName.Contains(TSqlDomainAttributes.NamePartSeparator)
                        && !target.Contains(TSqlDomainAttributes.NamePartSeparator))
                    {
                        target = string.Concat(TSqlDomainAttributes.DefaultSchemaPrefix, target);
                    }

                    // no worry if grants are on another object - a different rule controls it
                    if (!target.Equals(mainObject.ObjectFullName, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    // mix of names sec object / sec subject must be ignored
                    // if script is about creating sec object then all good
                    else if (mainObject.ObjectDefinitionNode is CreateRoleStatement
                    || mainObject.ObjectDefinitionNode is CreateUserStatement
                    || mainObject.ObjectDefinitionNode is CreateLoginStatement)
                    {
                        return;
                    }

                    HandleNodeError(nd);
                }, null);

            node.AcceptChildren(grantVisitor);
        }
    }
}
