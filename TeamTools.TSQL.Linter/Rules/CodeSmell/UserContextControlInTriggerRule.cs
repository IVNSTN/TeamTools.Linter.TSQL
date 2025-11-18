using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0188", "USER_CONTEXT_CONTROL_IN_TRIGGER")]
    [TriggerRule]
    [SecurityRule]
    internal sealed class UserContextControlInTriggerRule : AbstractRule
    {
        private readonly SetUserContextDetector usrContextDetector;

        public UserContextControlInTriggerRule() : base()
        {
            usrContextDetector = new SetUserContextDetector(ViolationHandler);
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is TriggerStatementBody trg)
            {
                DoValidate(trg);
            }
        }

        private static ExecuteAsTriggerOption DetectExecuteAs(IList<TriggerOption> options)
        {
            int n = options.Count;

            for (int i = 0; i < n; i++)
            {
                if (options[i] is ExecuteAsTriggerOption executeAs)
                {
                    return executeAs;
                }
            }

            return default;
        }

        private void DoValidate(TriggerStatementBody node)
        {
            HandleNodeErrorIfAny(DetectExecuteAs(node.Options));

            node.StatementList?.AcceptChildren(usrContextDetector);
        }

        private sealed class SetUserContextDetector : VisitorWithCallback
        {
            public SetUserContextDetector(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(ExecuteAsClause node) => Callback(node);

            public override void Visit(ExecuteAsStatement node) => Callback(node);

            public override void Visit(SetUserStatement node) => Callback(node);

            public override void Visit(RevertStatement node) => Callback(node);
        }
    }
}
