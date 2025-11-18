using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : collapse all similar _IN_CONSTRAINT rules into one
    [RuleIdentity("CD0282", "STRING_FN_IN_CONSTRAINT")]
    internal sealed class StringFunctionInConstraintRule : BaseConstraintDeploymentRestrictionRule
    {
        public StringFunctionInConstraintRule() : base(true)
        {
        }

        protected override TSqlFragmentVisitor MakeConstraintValidator() => new StringFunctionVisitor(ViolationHandler);

        private class StringFunctionVisitor : VisitorWithCallback
        {
            public StringFunctionVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(LeftFunctionCall node) => Callback(node);

            public override void Visit(RightFunctionCall node) => Callback(node);
        }
    }
}
