using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CD0472", "BETWEEN_IN_CONSTRAINT")]
    internal sealed class BetweenInConstraintRule : BaseConstraintDeploymentRestrictionRule
    {
        public BetweenInConstraintRule() : base()
        {
        }

        protected override TSqlFragmentVisitor MakeConstraintValidator() => new BetweenVisitor(ViolationHandler);

        private sealed class BetweenVisitor : VisitorWithCallback
        {
            public BetweenVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(BooleanTernaryExpression node) => Callback(node);
        }
    }
}
