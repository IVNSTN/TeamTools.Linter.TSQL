using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
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
