using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Proc specifics.
    /// </summary>
    internal partial class NativelyUnsupportedInstructionRule
    {
        private static readonly Dictionary<ProcedureOptionKind, string> UnsupportedProcOptions = new Dictionary<ProcedureOptionKind, string>
        {
            { ProcedureOptionKind.Encryption, "ENCRYPTION" },
            { ProcedureOptionKind.Recompile, "RECOMPILE" },
        };

        private void DoValidate(ProcedureStatementBody node)
        {
            if (!node.Options.HasOption(ProcedureOptionKind.NativeCompilation))
            {
                return;
            }

            if (node.ProcedureReference.Name.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                HandleNodeError(node.ProcedureReference.Name.BaseIdentifier, Strings.ViolationDetails_NativelyUnsupportedInstructionRule_TempProc);
            }

            if (node.IsForReplication)
            {
                HandleNodeError(node.Options.FirstOrDefault() as TSqlFragment ?? node, "FOR REPLICATION");
            }

            HandleNodeErrorIfAny(node.ProcedureReference.Number, Strings.ViolationDetails_NativelyUnsupportedInstructionRule_NumberedProc);

            DoValidateOptions(node.Options);
            DoValidateStatements(node, node.StatementList);
        }

        private void DoValidateOptions(IList<ProcedureOption> options)
        {
            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                var opt = options[i];
                if (UnsupportedProcOptions.TryGetValue(opt.OptionKind, out var optionName))
                {
                    HandleNodeError(opt, optionName);
                }
            }
        }
    }
}
