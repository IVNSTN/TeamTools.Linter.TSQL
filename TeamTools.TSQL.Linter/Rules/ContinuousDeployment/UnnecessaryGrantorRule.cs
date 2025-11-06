using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CD0213", "UNNECESSARY_GRANTOR")]
    [SecurityRule]
    internal sealed class UnnecessaryGrantorRule : AbstractRule
    {
        private readonly IList<SecurityObjectKind> checkedObjectKinds = new List<SecurityObjectKind>();

        public UnnecessaryGrantorRule() : base()
        {
            // sqplackage does not like these
            checkedObjectKinds.Add(SecurityObjectKind.Schema);
            checkedObjectKinds.Add(SecurityObjectKind.Type);
            checkedObjectKinds.Add(SecurityObjectKind.Service);
        }

        public override void Visit(GrantStatement node)
        {
            if (node.SecurityTargetObject == null)
            {
                // db/server level permissions
                return;
            }

            if (!checkedObjectKinds.Contains(node.SecurityTargetObject.ObjectKind))
            {
                return;
            }

            if (string.IsNullOrEmpty(node.AsClause?.Value))
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
