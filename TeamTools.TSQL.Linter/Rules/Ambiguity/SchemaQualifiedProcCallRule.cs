using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0104", "EXEC_PROC_SCHEMA")]
    internal sealed class SchemaQualifiedProcCallRule : AbstractRule
    {
        public SchemaQualifiedProcCallRule() : base()
        {
        }

        public override void Visit(ExecutableProcedureReference node)
        {
            // no check if proc is identified by variable
            if (node.ProcedureReference.ProcedureVariable != null)
            {
                return;
            }

            var name = node.ProcedureReference.ProcedureReference.Name;

            // system proc
            if (SystemProcDetector.IsSystemProc(name.BaseIdentifier.Value))
            {
                return;
            }

            // schema is defined
            if ((name.SchemaIdentifier != null)
            && !string.IsNullOrEmpty(name.SchemaIdentifier.Value))
            {
                return;
            }

            if (InvisibleCharDetector.LocateInvisibleChar(name.BaseIdentifier.Value, out var _) == 0)
            {
                // This is an invisible unicode symbol which is supposed to be detected by a separate rule.
                // The parser treats it as an identifier and parses such invisible thing as ExecuteStatement
                // with [EXEC] keyword omitted and "empty" (invisible) proc name - such cases should be ignored.
                return;
            }

            HandleNodeError(name);
        }
    }
}
