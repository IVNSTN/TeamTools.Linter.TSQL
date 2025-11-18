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
        private static readonly HashSet<string> ForbiddenProcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sp_getapplock",
            "sp_releaseapplock",
        };

        private readonly ForbiddenProcCallVisitor procVisitor;

        public AppLockRule() : base()
        {
            procVisitor = new ForbiddenProcCallVisitor(ViolationHandler, ForbiddenProcs);
        }

        protected override void ValidateScript(TSqlScript node) => node.Accept(procVisitor);
    }
}
