using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : collapse all similar _IN_CONSTRAINT rules into one, rename with respect to computed columns
    [RuleIdentity("CD0277", "DATE_FN_IN_CONSTRAINT")]
    internal sealed class DateFunctionsInConstraintRule : AbstractRule
    {
        public DateFunctionsInConstraintRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node) => DoValidate(node.SchemaObjectName, node);

        public override void Visit(AlterTableStatement node) => DoValidate(node.SchemaObjectName, node);

        public override void Visit(CreateTypeTableStatement node) => DoValidate(node);

        private void DoValidate(SchemaObjectName name, TSqlFragment node)
        {
            if (name.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                // ignoring #
                return;
            }

            DoValidate(node);
        }

        private void DoValidate(TSqlFragment node)
        {
            var cstrVisitor = new ConstraintDefinitionValidator(() => new DateFunctionVisitor(), HandleNodeError);
            node.AcceptChildren(cstrVisitor);

            var computeVisitor = new ComputedColumnValidator(() => new DateFunctionVisitor(), HandleNodeError);
            node.AcceptChildren(computeVisitor);
        }

        private class DateFunctionVisitor : TSqlViolationDetector
        {
            private static readonly ICollection<string> ForbiddenFunctions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "YEAR",
                "MONTH",
                "DAY",
            };

            public override void Visit(FunctionCall node)
            {
                if (ForbiddenFunctions.Contains(node.FunctionName.Value))
                {
                    MarkDetected(node);
                }
            }
        }
    }
}
