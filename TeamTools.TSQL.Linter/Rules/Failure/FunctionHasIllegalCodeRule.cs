using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0978", "FUNCTION_HAS_ILLEGAL_CODE")]
    internal sealed class FunctionHasIllegalCodeRule : AbstractRule
    {
        private readonly IllegalStatementVisitor detector;

        public FunctionHasIllegalCodeRule() : base()
        {
            detector = new IllegalStatementVisitor(HandleNodeError);
        }

        public override void Visit(FunctionStatementBody node)
        {
            node.AcceptChildren(detector);
        }

        private class IllegalStatementVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> callback;

            public IllegalStatementVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(ExecutableProcedureReference node) => callback(node, "EXEC");

            public override void Visit(ThrowStatement node) => callback(node, "THROW");

            public override void Visit(RaiseErrorLegacyStatement node) => callback(node, "RAISERROR");

            public override void Visit(RaiseErrorStatement node) => callback(node, "RAISERROR");
        }
    }
}
