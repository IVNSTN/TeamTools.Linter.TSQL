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
            else if (firstStmt is FunctionStatementBody fn)
            {
                DoValidate(fn);
            }
        }

        private void DoValidate(FunctionStatementBody node)
        {
            // FIXME : in many places clr functions are detected with 'node.StatementList is null' predicate
            // however it appears that inline table functions have this property 'null' as well
            // the query is defined in function result type property.
            // Maybe some rules unexpectedly ignore inline function bodies.
            if (node.MethodSpecifier is null)
            {
                // not external func
                return;
            }

            if (node.Options is null)
            {
                return;
            }

            int n = node.Options.Count;
            for (int i = 0; i < n; i++)
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

            if (node.Options is null)
            {
                return;
            }

            if (node.IsForReplication)
            {
                // TODO : point to more specific place
                HandleNodeError(node);
            }

            int n = node.Options.Count;
            for (int i = 0; i < n; i++)
            {
                var opt = node.Options[i];
                if (BadProcOptions.TryGetValue(opt.OptionKind, out var optionName))
                {
                    HandleNodeError(opt, optionName);
                }
            }
        }
    }
}
