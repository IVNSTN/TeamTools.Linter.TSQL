using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0733", "DECLARE_IN_BLOCK")]
    internal sealed class DeclareInCodeBlockRule : AbstractRule
    {
        private readonly DeclareDetector declareDetector;

        public DeclareInCodeBlockRule() : base()
        {
            declareDetector = new DeclareDetector(ViolationHandler);
        }

        // TODO : avoid double-visiting of nested elements
        public override void Visit(IfStatement node) => DetectNestedDeclare(node);

        public override void Visit(WhileStatement node) => DetectNestedDeclare(node);

        public override void Visit(TryCatchStatement node) => DetectNestedDeclare(node);

        private void DetectNestedDeclare(TSqlFragment node) => node.AcceptChildren(declareDetector);

        private sealed class DeclareDetector : VisitorWithCallback
        {
            public DeclareDetector(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(DeclareVariableStatement node) => Callback(node);

            public override void Visit(DeclareTableVariableStatement node) => Callback(node);
        }
    }
}
