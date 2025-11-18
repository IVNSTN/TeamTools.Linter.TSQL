using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0823", "RECOMPILE_RECOMPILE")]
    internal sealed class DoubleRecompileRule : AbstractRule
    {
        private readonly OptionRecompileVisitor visitor;

        public DoubleRecompileRule() : base()
        {
            visitor = new OptionRecompileVisitor(ViolationHandler);
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                DoValidate(proc);
            }
        }

        private void DoValidate(ProcedureStatementBody node)
        {
            if (node.StatementList is null)
            {
                // CLR
                return;
            }

            if (node.Options is null || !node.Options.HasOption(ProcedureOptionKind.Recompile))
            {
                // no proc-level recompile
                return;
            }

            node.Accept(visitor);
        }

        private class OptionRecompileVisitor : VisitorWithCallback
        {
            public OptionRecompileVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(OptimizerHint node)
            {
                if (node.HintKind == OptimizerHintKind.Recompile)
                {
                    Callback(node);
                }
            }
        }
    }
}
