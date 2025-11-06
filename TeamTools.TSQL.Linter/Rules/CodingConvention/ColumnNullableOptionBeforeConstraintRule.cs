using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0217", "COLUMN_NULLABILITY_BEFORE_CONSTRAINT")]
    internal sealed class ColumnNullableOptionBeforeConstraintRule : AbstractRule
    {
        public ColumnNullableOptionBeforeConstraintRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node)
        {
            if (node.ComputedColumnExpression != null)
            {
                return;
            }

            var nullConstraint = node.Constraints.OfType<NullableConstraintDefinition>().FirstOrDefault();
            if (nullConstraint is null)
            {
                return;
            }

            // Nullability first, IDENTITY attribute after
            ValidatePosition(nullConstraint, node.IdentityOptions);
            // Nullability first, default constraint after
            ValidatePosition(nullConstraint, node.DefaultConstraint);

            foreach (var constraint in node.Constraints.Where(cs => cs != nullConstraint))
            {
                ValidatePosition(nullConstraint, constraint);
            }
        }

        private void ValidatePosition(NullableConstraintDefinition nullConstraint, TSqlFragment otherConstraint)
        {
            if (otherConstraint != null && otherConstraint.FirstTokenIndex < nullConstraint.FirstTokenIndex)
            {
                HandleNodeError(nullConstraint);
            }
        }
    }
}
