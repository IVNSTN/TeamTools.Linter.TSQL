using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Proc specifics.
    /// </summary>
    internal partial class NativelyUnsupportedInstructionRule
    {
        private static readonly ICollection<ProcedureOptionKind> UnsupportedProcOptions = new List<ProcedureOptionKind>
        {
            ProcedureOptionKind.Encryption,
            ProcedureOptionKind.Recompile,
        };

        public override void Visit(ProcedureStatementBody node)
        {
            if (!node.Options.Any(opt => opt.OptionKind == ProcedureOptionKind.NativeCompilation))
            {
                return;
            }

            if (node.ProcedureReference.Name.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                HandleNodeError(node.ProcedureReference.Name.BaseIdentifier, "temp proc");
            }

            if (node.IsForReplication)
            {
                HandleNodeError(node.Options.FirstOrDefault() as TSqlFragment ?? node, "FOR REPLICATION");
            }

            HandleNodeErrorIfAny(node.ProcedureReference.Number, "numbered proc");

            node.Options
                .Where(opt => UnsupportedProcOptions.Contains(opt.OptionKind))
                .ToList()
                .ForEach(opt => HandleNodeError(opt, opt.OptionKind.ToString().ToUpperInvariant()));

            DoValidateStatements(node, node.StatementList);
        }
    }
}
