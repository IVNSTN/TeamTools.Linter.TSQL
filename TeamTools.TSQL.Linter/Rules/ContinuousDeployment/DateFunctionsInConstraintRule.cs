using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : collapse all similar _IN_CONSTRAINT rules into one, rename with respect to computed columns
    [RuleIdentity("CD0277", "DATE_FN_IN_CONSTRAINT")]
    internal sealed class DateFunctionsInConstraintRule : BaseConstraintDeploymentRestrictionRule
    {
        public DateFunctionsInConstraintRule() : base()
        {
        }

        protected override TSqlFragmentVisitor MakeConstraintValidator() => new DateFunctionVisitor(ViolationHandler);

        private class DateFunctionVisitor : VisitorWithCallback
        {
            private static readonly HashSet<string> ForbiddenFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "YEAR",
                "MONTH",
                "DAY",
            };

            public DateFunctionVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(FunctionCall node)
            {
                if (ForbiddenFunctions.Contains(node.FunctionName.Value))
                {
                    Callback(node);
                }
            }
        }
    }
}
