using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0521", "SYSPROC_RETURN_NOT_CHECKED")]
    internal sealed partial class SystemProcedureReturnCodeCheckRule : AbstractRule
    {
        public SystemProcedureReturnCodeCheckRule() : base()
        {
        }

        public override void Visit(ExecuteSpecification node)
        {
            // return value is received
            if (node.Variable != null)
            {
                return;
            }

            if (!(node.ExecutableEntity is ExecutableProcedureReference procRef)
            || procRef.ProcedureReference.ProcedureVariable != null)
            {
                // no check if proc is identified by variable
                return;
            }

            var procFullName = procRef.ProcedureReference.ProcedureReference.Name;

            if (!IsSystemProcWithReturnCode(procFullName))
            {
                return;
            }

            if (IsIgnorableProc(procFullName))
            {
                return;
            }

            HandleNodeError(node, procFullName.BaseIdentifier.Value);
        }

        private static bool IsIgnorableProc(SchemaObjectName name)
        {
            if (name.SchemaIdentifier != null
            && !name.SchemaIdentifier.Value.Equals(TSqlDomainAttributes.SystemSchemaName, StringComparison.OrdinalIgnoreCase))
            {
                // not [sys] proc
                return true;
            }

            return IgnorableProcs.Contains(name.BaseIdentifier.Value);
        }

        private bool IsSystemProcWithReturnCode(SchemaObjectName name)
            => systemProcsWithReturnCode.Contains(name.BaseIdentifier.Value);
    }
}
