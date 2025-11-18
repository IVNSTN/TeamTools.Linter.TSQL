using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
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

            var nullConstraint = FindNullConstraint(node.Constraints);
            if (nullConstraint is null)
            {
                return;
            }

            // Nullability first, IDENTITY attribute after
            ValidatePosition(nullConstraint, node.IdentityOptions);
            // Nullability first, default constraint after
            ValidatePosition(nullConstraint, node.DefaultConstraint);

            int n = node.Constraints.Count;

            if (n < 2)
            {
                return;
            }

            for (int i = 0; i < n; i++)
            {
                ValidatePosition(nullConstraint, node.Constraints[i]);
            }
        }

        private static NullableConstraintDefinition FindNullConstraint(IList<ConstraintDefinition> constraints)
        {
            int n = constraints.Count;
            for (int i = 0; i < n; i++)
            {
                if (constraints[i] is NullableConstraintDefinition cstr)
                {
                    return cstr;
                }
            }

            return default;
        }

        private void ValidatePosition(NullableConstraintDefinition nullConstraint, TSqlFragment otherConstraint)
        {
            if (otherConstraint is null)
            {
                return;
            }

            if (nullConstraint == otherConstraint)
            {
                return;
            }

            if (otherConstraint.FirstTokenIndex > nullConstraint.FirstTokenIndex)
            {
                return;
            }

            HandleNodeError(nullConstraint);
        }
    }
}
