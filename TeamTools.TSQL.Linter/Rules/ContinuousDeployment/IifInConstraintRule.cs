using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : collapse all similar _IN_CONSTRAINT rules into one, rename with respect to computed columns
    [RuleIdentity("CD0280", "IIF_IN_CONSTRAINT")]
    [CompatibilityLevel(SqlVersion.Sql110)]
    internal sealed class IifInConstraintRule : BaseConstraintDeploymentRestrictionRule
    {
        public IifInConstraintRule() : base()
        {
        }

        protected override TSqlFragmentVisitor MakeConstraintValidator() => new IifFunctionVisitor(ViolationHandler);

        private sealed class IifFunctionVisitor : VisitorWithCallback
        {
            public IifFunctionVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(IIfCall node) => Callback(node);
        }
    }
}
