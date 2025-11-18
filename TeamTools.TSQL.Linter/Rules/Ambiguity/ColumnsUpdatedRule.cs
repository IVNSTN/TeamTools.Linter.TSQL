using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0836", "COLUMNS_UPDATED")]
    [TriggerRule]
    internal sealed class ColumnsUpdatedRule : AbstractRule
    {
        public ColumnsUpdatedRule() : base()
        {
        }

        public override void Visit(TriggerStatementBody node) => DetectColumnsUpdated(node.StatementList);

        private void DetectColumnsUpdated(StatementList body)
        {
            if ((body?.Statements?.Count ?? 0) == 0)
            {
                return;
            }

            body.Accept(new ColumnsUpdatedVisitor(ViolationHandler));
        }

        private sealed class ColumnsUpdatedVisitor : VisitorWithCallback
        {
            private static readonly string ColumnsUpdatedFunction = "COLUMNS_UPDATED";

            public ColumnsUpdatedVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(FunctionCall node)
            {
                if (node.FunctionName.Value.Equals(ColumnsUpdatedFunction))
                {
                    Callback(node);
                }
            }
        }
    }
}
