using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0102", "POSITIONAL_PROC_ARGS")]
    internal sealed class PositionalProcArgumentsRule : AbstractRule
    {
        public PositionalProcArgumentsRule() : base()
        {
        }

        public override void Visit(ExecutableProcedureReference node)
        {
            if ((node.ProcedureReference.ProcedureVariable is null)
            && SystemProcDetector.IsSystemProc(node.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value))
            {
                // some system procs don't allow named arguments
                return;
            }

            for (int i = node.Parameters.Count - 1; i >= 0; i--)
            {
                var p = node.Parameters[i];
                if (p.Variable is null)
                {
                    HandleNodeError(p);
                }
            }
        }
    }
}
