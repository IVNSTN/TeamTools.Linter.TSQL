using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CD0213", "UNNECESSARY_GRANTOR")]
    [SecurityRule]
    internal sealed class UnnecessaryGrantorRule : AbstractRule
    {
        public UnnecessaryGrantorRule() : base()
        {
        }

        public override void Visit(GrantStatement node)
        {
            if (string.IsNullOrEmpty(node.AsClause?.Value))
            {
                // grantor not defined
                return;
            }

            if (node.SecurityTargetObject is null)
            {
                // db/server level permissions
                return;
            }

            // sqplackage does not like this, see GrantOnSchemaGrantorRule
            // see also GrantorRule
            if (!IsSupportedTargetObject(node.SecurityTargetObject.ObjectKind))
            {
                return;
            }

            HandleNodeError(node);
        }

        private static bool IsSupportedTargetObject(SecurityObjectKind objType)
        {
            return objType == SecurityObjectKind.Schema
                || objType == SecurityObjectKind.Type
                || objType == SecurityObjectKind.Service;
        }
    }
}
