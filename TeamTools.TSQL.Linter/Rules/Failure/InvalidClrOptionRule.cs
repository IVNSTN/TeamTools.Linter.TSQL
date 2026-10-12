using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0822", "INVALID_CLR_OPTION")]
    internal sealed class InvalidClrOptionRule : AbstractRule
    {
        private static readonly Dictionary<FunctionOptionKind, string> BadFuncOptions = new Dictionary<FunctionOptionKind, string>
        {
            { FunctionOptionKind.Encryption, "ENCRYPTION" },
        };

        private static readonly Dictionary<TriggerOptionKind, string> BadTrgOptions = new Dictionary<TriggerOptionKind, string>
        {
            { TriggerOptionKind.Encryption, "ENCRYPTION" },
        };

        private static readonly Dictionary<ProcedureOptionKind, string> BadProcOptions = new Dictionary<ProcedureOptionKind, string>
        {
            { ProcedureOptionKind.Encryption, "ENCRYPTION" },
            { ProcedureOptionKind.Recompile, "RECOMPILE" },
        };

        public InvalidClrOptionRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                DoValidate(proc);
            }
            else if (firstStmt is TriggerStatementBody tr)
            {
                DoValidate(tr);
            }
            else if (firstStmt is FunctionStatementBody fn)
            {
                DoValidate(fn);
            }
        }

        private void DoValidate(FunctionStatementBody node)
        {
            // Not only CLR modules but also inline functions have no StatementList.
            // We need specifically CLR modules only here.
            if (node.MethodSpecifier is null)
            {
                // not external func
                return;
            }

            for (int i = node.Options.Count - 1; i >= 0; i--)
            {
                var opt = node.Options[i];
                if (BadFuncOptions.TryGetValue(opt.OptionKind, out var optionName))
                {
                    HandleNodeError(opt, optionName);
                }
            }
        }

        private void DoValidate(ProcedureStatementBody node)
        {
            if (node.MethodSpecifier is null)
            {
                // not external proc
                return;
            }

            if (node.IsForReplication)
            {
                HandleNodeError(node.MethodSpecifier);
            }

            for (int i = node.Options.Count - 1; i >= 0; i--)
            {
                var opt = node.Options[i];
                if (BadProcOptions.TryGetValue(opt.OptionKind, out var optionName))
                {
                    HandleNodeError(opt, optionName);
                }
            }
        }

        private void DoValidate(TriggerStatementBody node)
        {
            if (node.MethodSpecifier is null)
            {
                // not external trigger
                return;
            }

            for (int i = node.Options.Count - 1; i >= 0; i--)
            {
                var opt = node.Options[i];
                if (BadTrgOptions.TryGetValue(opt.OptionKind, out var optionName))
                {
                    HandleNodeError(opt, optionName);
                }
            }
        }
    }
}
