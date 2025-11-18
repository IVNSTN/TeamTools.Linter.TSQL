using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0408", "DEPRECATED_SYS_VIEW")]
    internal sealed class DeprecatedSystemViewRule : AbstractRule
    {
        // TODO : move to SqlServerMetadata or deprecation config
        private static readonly Dictionary<string, string> DeprecatedViews = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "dbo.syscolumns", "sys.columns" },
            { "dbo.sysmembers", "sys.database_role_members" },
            { "dbo.sysobjects", "sys.objects" },
            { "dbo.sysprotects", "sys.database_permissions" },
            { "dbo.systypes", "sys.types" },
            { "dbo.sysusers", "sys.database_principals" },
            { "dbo.syscacheobjects", "sys.dm_exec_plan_attributes" },
            { "dbo.sysprocesses", "sys.dm_exec_requests" },
        };

        public DeprecatedSystemViewRule() : base()
        {
        }

        public override void Visit(NamedTableReference node)
        {
            var viewName = node.SchemaObject.GetFullName();
            if (DeprecatedViews.TryGetValue(viewName, out var replacement))
            {
                HandleNodeError(node, string.Format(Strings.ViolationDetails_DeprecatedSystemViewRule_UseOtherOption, replacement));
            }
        }
    }
}
