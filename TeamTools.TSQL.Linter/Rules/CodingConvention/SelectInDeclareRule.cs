using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0227", "SELECT_IN_DECLARE")]
    internal sealed class SelectInDeclareRule : AbstractRule
    {
        private readonly SelectVisitor visitor;

        public SelectInDeclareRule() : base()
        {
            visitor = new SelectVisitor(ViolationHandler);
        }

        public override void Visit(DeclareVariableStatement node) => node.Accept(visitor);

        private sealed class SelectVisitor : VisitorWithCallback
        {
            public SelectVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(QueryExpression node) => Callback(node);
        }
    }
}
