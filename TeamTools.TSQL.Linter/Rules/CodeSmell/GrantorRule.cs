using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0125", "GRANT_GRANTOR")]
    internal sealed class GrantorRule : AbstractRule
    {
        private static readonly Lazy<IList<SecurityObjectKind>> IgnoredObjectKindsInstance
            = new Lazy<IList<SecurityObjectKind>>(() => InitIgnoredObjectKindsInstance(), true);

        public GrantorRule() : base()
        {
        }

        private static IList<SecurityObjectKind> IgnoredObjectKinds => IgnoredObjectKindsInstance.Value;

        public override void Visit(GrantStatement node)
        {
            if (!string.IsNullOrEmpty(node.AsClause?.Value))
            {
                return;
            }

            if (node.SecurityTargetObject == null)
            {
                // db/server level permissions
                return;
            }

            // TODO : extract to grantor exceptions list
            // sqplackage does not like this, see GrantOnSchemaGrantorRule
            if (IgnoredObjectKinds.Contains(node.SecurityTargetObject.ObjectKind))
            {
                return;
            }

            HandleNodeError(node);
        }

        private static IList<SecurityObjectKind> InitIgnoredObjectKindsInstance()
        {
            return new List<SecurityObjectKind>
            {
                SecurityObjectKind.Schema,
                SecurityObjectKind.Type,
                SecurityObjectKind.Service,
            };
        }
    }
}
