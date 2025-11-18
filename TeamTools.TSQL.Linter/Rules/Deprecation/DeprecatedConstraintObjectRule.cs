using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0406", "DEPRECATED_CONSTRAINT")]
    internal sealed class DeprecatedConstraintObjectRule : AbstractRule
    {
        private static readonly Lazy<HashSet<string>> ForbiddenProcsInstance
            = new Lazy<HashSet<string>>(() => InitForbiddenProcsInstance(), true);

        public DeprecatedConstraintObjectRule() : base()
        {
        }

        private static HashSet<string> ForbiddenProcs => ForbiddenProcsInstance.Value;

        public override void Visit(CreateRuleStatement node) => HandleNodeError(node);

        public override void Visit(CreateDefaultStatement node) => HandleNodeError(node);

        public override void Visit(DropRuleStatement node) => HandleNodeError(node);

        public override void Visit(DropDefaultStatement node) => HandleNodeError(node);

        public override void Visit(ExecutableProcedureReference node)
        {
            if (node.ProcedureReference.ProcedureVariable != null)
            {
                return;
            }

            string procName = node.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value;

            if (ForbiddenProcs.Contains(procName))
            {
                HandleNodeError(node);
            }
        }

        private static HashSet<string> InitForbiddenProcsInstance()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "sp_binddefault",
                "sp_unbinddefault",
                "sp_bindrule",
                "sp_unbindrule",
            };
        }
    }
}
