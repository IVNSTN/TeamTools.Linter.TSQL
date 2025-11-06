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
        private static readonly Lazy<ICollection<string>> IgnoredSystemProcsInstance
            = new Lazy<ICollection<string>>(() => InitIgnoredSystemProcsInstance(), true);

        private readonly SystemProcDetector systemProcDetector = new SystemProcDetector();

        public SystemProcedureReturnCodeCheckRule() : base()
        {
        }

        private static ICollection<string> IgnoredSystemProcs => IgnoredSystemProcsInstance.Value;

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
            if (!systemProcDetector.IsSystemProc(procName))
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

        private static ICollection<string> InitIgnoredSystemProcsInstance()
        {
            // TODO : consolidate all the metadata about known built-in functions
            return new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "sp_addextendedproperty",
                "sp_executesql",
                "sp_bindefault",
                "sp_unbindefault",
                "sp_bindrule",
                "sp_unbindrule",
            };
        }
    }
}
