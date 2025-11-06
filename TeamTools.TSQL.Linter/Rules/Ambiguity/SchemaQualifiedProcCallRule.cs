using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0104", "EXEC_PROC_SCHEMA")]
    internal sealed class SchemaQualifiedProcCallRule : AbstractRule
    {
        private readonly SystemProcDetector systemProcDetector;

        public SchemaQualifiedProcCallRule() : base()
        {
            systemProcDetector = new SystemProcDetector();
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
            if (systemProcDetector.IsSystemProc(name.BaseIdentifier.Value))
            {
                return;
            }

            // schema is defined
            if ((name.SchemaIdentifier != null)
            && !string.IsNullOrEmpty(name.SchemaIdentifier.Value))
            {
                return;
            }

            HandleNodeError(name);
        }
    }
}
