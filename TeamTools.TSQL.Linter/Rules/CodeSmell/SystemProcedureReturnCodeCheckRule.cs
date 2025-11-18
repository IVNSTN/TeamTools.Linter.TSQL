using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0521", "SYSPROC_RETURN_NOT_CHECKED")]
    internal sealed class SystemProcedureReturnCodeCheckRule : AbstractRule
    {
        private static readonly HashSet<string> IgnoredSystemProcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sp_addextendedproperty",
            "sp_executesql",
            "sp_bindefault",
            "sp_unbindefault",
            "sp_bindrule",
            "sp_unbindrule",
        };

        public SystemProcedureReturnCodeCheckRule() : base()
        {
        }

        public override void Visit(ExecuteSpecification node)
        {
            // there is var for return value
            if (null != node.Variable)
            {
                return;
            }

            if (!(node.ExecutableEntity is ExecutableProcedureReference procRef))
            {
                return;
            }

            // no check if proc is identified by variable
            if (procRef.ProcedureReference.ProcedureVariable != null)
            {
                return;
            }

            string procName = procRef.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value;

            // system proc only
            if (!SystemProcDetector.IsSystemProc(procName))
            {
                return;
            }

            // not reporting on what's ignored
            if (IgnoredSystemProcs.Contains(procName))
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
