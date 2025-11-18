using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0125", "GRANT_GRANTOR")]
    internal sealed class GrantorRule : AbstractRule
    {
        public GrantorRule() : base()
        {
        }

        public override void Visit(GrantStatement node)
        {
            if (!string.IsNullOrEmpty(node.AsClause?.Value))
            {
                // grantor defined
                return;
            }

            if (node.SecurityTargetObject is null)
            {
                // db/server level permissions
                return;
            }

            // sqplackage does not like these, see also UnnecessaryGrantorRule
            if (IsIgnorableTargetObject(node.SecurityTargetObject.ObjectKind))
            {
                return;
            }

            HandleNodeError(node);
        }

        private static bool IsIgnorableTargetObject(SecurityObjectKind objType)
        {
            return objType == SecurityObjectKind.Schema
                || objType == SecurityObjectKind.Type
                || objType == SecurityObjectKind.Service;
        }
    }
}
