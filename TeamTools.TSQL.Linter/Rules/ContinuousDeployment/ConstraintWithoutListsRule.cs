using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : collapse all similar _IN_CONSTRAINT rules into one, rename with respect to computed columns
    [RuleIdentity("CD0214", "LIST_IN_CONSTRAINT")]
    internal sealed class ConstraintWithoutListsRule : BaseConstraintDeploymentRestrictionRule
    {
        public ConstraintWithoutListsRule() : base()
        {
        }

        protected override TSqlFragmentVisitor MakeConstraintValidator() => new InDetector(ViolationHandler);

        private sealed class InDetector : VisitorWithCallback
        {
            public InDetector(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(InPredicate node) => Callback(node);
        }
    }
}
