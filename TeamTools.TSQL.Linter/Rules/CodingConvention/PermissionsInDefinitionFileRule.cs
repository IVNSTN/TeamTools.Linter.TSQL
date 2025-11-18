using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0165", "PERMISSION_IN_DEFINITION")]
    [SecurityRule]
    internal sealed class PermissionsInDefinitionFileRule : ScriptAnalysisServiceConsumingRule
    {
        // TODO : very similar to PermissionMisdirectedRule
        public PermissionsInDefinitionFileRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var mainObject = GetService<MainScriptObjectDetector>(node);
            if (string.IsNullOrWhiteSpace(mainObject?.ObjectFullName))
            {
                return;
            }

            var targetValidator = new Action<SecurityStatement, string>((nd, target) => ValidateTarget(nd, target, mainObject, ViolationHandler));
            var grantVisitor = new GrantTargetVisitor(targetValidator, null);
            node.Accept(grantVisitor);
        }

        private static void ValidateTarget(SecurityStatement node, string target, MainScriptObjectDetector mainObject, Action<TSqlFragment> callback)
        {
            if (string.IsNullOrEmpty(target))
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

            callback(node);
        }
    }
}
