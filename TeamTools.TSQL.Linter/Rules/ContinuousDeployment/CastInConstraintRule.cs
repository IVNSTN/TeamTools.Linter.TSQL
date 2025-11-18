using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : collapse all similar _IN_CONSTRAINT rules into one, rename with respect to computed columns
    [RuleIdentity("CD0216", "CAST_IN_CONSTRAINT")]
    internal sealed class CastInConstraintRule : BaseConstraintDeploymentRestrictionRule
    {
        public CastInConstraintRule() : base()
        {
        }

        protected override TSqlFragmentVisitor MakeConstraintValidator() => new CastVisitor(ViolationHandler);

        private sealed class CastVisitor : VisitorWithCallback
        {
            public CastVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(CastCall node) => Callback(node);
        }
    }
}
