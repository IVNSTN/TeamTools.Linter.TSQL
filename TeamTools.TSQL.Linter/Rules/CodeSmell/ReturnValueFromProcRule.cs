using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0142", "PROC_RETURN_VALUE_REQUIRED")]

    internal sealed class ReturnValueFromProcRule : AbstractRule
    {
        public ReturnValueFromProcRule() : base()
        {
        }

        public override void Visit(ProcedureStatementBody node)
        {
            var returnVisitor = new ReturnVisitor(HandleNodeError);
            node.AcceptChildren(returnVisitor);
        }

        private class ReturnVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> handleNodeError;

            public ReturnVisitor(Action<TSqlFragment, string> errHandler)
            {
                handleNodeError = errHandler;
            }

            public override void Visit(ReturnStatement node)
            {
                if (null != node.Expression)
                {
                    return;
                }

                handleNodeError(node, "");
            }
        }
    }
}
