using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0142", "PROC_RETURN_VALUE_REQUIRED")]

    internal sealed class ReturnValueFromProcRule : AbstractRule
    {
        private readonly ReturnVisitor returnVisitor;

        public ReturnValueFromProcRule() : base()
        {
            returnVisitor = new ReturnVisitor(ViolationHandler);
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                proc.StatementList?.AcceptChildren(returnVisitor);
            }
        }

        private sealed class ReturnVisitor : VisitorWithCallback
        {
            public ReturnVisitor(Action<TSqlFragment> callback) : base(callback)
            {
            }

            public override void Visit(ReturnStatement node)
            {
                if (node.Expression != null)
                {
                    return;
                }

                Callback(node);
            }
        }
    }
}
