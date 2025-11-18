using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0131", "SET_OPTION_ILLEGAL")]
    internal sealed class SetOptionInFunctionRule : AbstractRule
    {
        private readonly SetOptionVisitor setOptionDetector;

        public SetOptionInFunctionRule() : base()
        {
            setOptionDetector = new SetOptionVisitor(ViolationHandler);
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is FunctionStatementBody fn)
            {
                fn.StatementList?.AcceptChildren(setOptionDetector);
            }
        }

        private class SetOptionVisitor : VisitorWithCallback
        {
            public SetOptionVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(PredicateSetStatement node) => Callback(node);
        }
    }
}
