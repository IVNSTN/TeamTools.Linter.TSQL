using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal abstract class BaseConstraintDeploymentRestrictionRule : AbstractRule
    {
        private readonly ConstraintDetector detector;

        protected BaseConstraintDeploymentRestrictionRule(bool ignoreComputedCols = false)
        {
            detector = new ConstraintDetector(MakeConstraintValidator(), ignoreComputedCols);
        }

        public override void Visit(CreateTypeTableStatement node) => node.Definition.AcceptChildren(detector);

        public override void Visit(CreateTableStatement node)
        {
            if (node.AsFileTable)
            {
                return;
            }

            if (node.SchemaObjectName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                return;
            }

            node.Definition.AcceptChildren(detector);
        }

        public override void Visit(AlterTableAddTableElementStatement node)
        {
            if (node.SchemaObjectName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                return;
            }

            node.Definition.AcceptChildren(detector);
        }

        protected abstract TSqlFragmentVisitor MakeConstraintValidator();

        private sealed class ConstraintDetector : TSqlFragmentVisitor
        {
            private readonly bool ignoreComputedCols;
            private readonly TSqlFragmentVisitor validator;

            public ConstraintDetector(TSqlFragmentVisitor validator, bool ignoreComputedCols)
            {
                this.validator = validator;
                this.ignoreComputedCols = ignoreComputedCols;
            }

            public override void Visit(DefaultConstraintDefinition node) => node.Expression.Accept(validator);

            public override void Visit(CheckConstraintDefinition node) => node.CheckCondition.Accept(validator);

            public override void Visit(ColumnDefinition node)
            {
                if (!ignoreComputedCols)
                {
                    node.ComputedColumnExpression?.Accept(validator);
                }
            }
        }
    }
}
