using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0119", "RETURN_VALUE_ILLEGAL")]
    internal sealed class ReturnValueIllegalRule : AbstractRule
    {
        private readonly ReturnValueVisitor returnVisitor;

        public ReturnValueIllegalRule() : base()
        {
            returnVisitor = new ReturnValueVisitor(ViolationHandler);
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is FunctionStatementBody fn)
            {
                DoValidate(fn);
            }
            else if (firstStmt is TriggerStatementBody tr)
            {
                DoValidate(tr);
            }
        }

        private void DoValidate(TriggerStatementBody node) => DetectIllegalValue(node.StatementList);

        private void DoValidate(FunctionStatementBody node)
        {
            if (node.ReturnType is ScalarFunctionReturnType)
            {
                return;
            }

            DetectIllegalValue(node.StatementList);
        }

        private void DetectIllegalValue(TSqlFragment node) => node?.AcceptChildren(returnVisitor);

        private sealed class ReturnValueVisitor : VisitorWithCallback
        {
            public ReturnValueVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void ExplicitVisit(ReturnStatement node)
            {
                if (node.Expression is null)
                {
                    return;
                }

                Callback(node);
            }
        }
    }
}
