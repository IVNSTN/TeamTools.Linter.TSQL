using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0520", "APP_LOCK")]
    internal sealed class AppLockRule : AbstractRule
    {
        private static readonly ICollection<string> ForbiddenProcs;

        static AppLockRule()
        {
            ForbiddenProcs = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "sp_getapplock",
                "sp_releaseapplock",
            };
        }

        public AppLockRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var procVisitor = new ForbiddenProcCallVisitor(HandleNodeError, ForbiddenProcs);
            node.AcceptChildren(procVisitor);
        }
    }
}
