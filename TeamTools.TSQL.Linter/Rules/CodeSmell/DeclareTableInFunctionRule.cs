using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0832", "MAKING_TABLES_IN_FUNC")]
    internal sealed class DeclareTableInFunctionRule : AbstractRule
    {
        private readonly DeclareTableVisitor declareDetector;

        public DeclareTableInFunctionRule() : base()
        {
            declareDetector = new DeclareTableVisitor(ViolationHandler);
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is FunctionStatementBody fn)
            {
                DoValidate(fn);
            }
        }

        private void DoValidate(FunctionStatementBody node)
        {
            if (node.MethodSpecifier != null)
            {
                // CLR
                return;
            }

            if (node.StatementList is null)
            {
                // inline table function
                return;
            }

            node.StatementList.Accept(declareDetector);
        }

        private class DeclareTableVisitor : VisitorWithCallback
        {
            public DeclareTableVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(DeclareTableVariableStatement node) => Callback(node);
        }
    }
}
