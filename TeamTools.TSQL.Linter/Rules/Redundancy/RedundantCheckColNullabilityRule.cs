using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0927", "REDUNDANT_COL_NULLABILITY_CHECK")]
    internal sealed class RedundantCheckColNullabilityRule : AbstractRule
    {
        public RedundantCheckColNullabilityRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node)
            => ValidateConstraints(node.Constraints, node.ColumnIdentifier);

        public override void Visit(CreateTableStatement node)
            => ValidateConstraints(node.Definition?.TableConstraints, node.SchemaObjectName.BaseIdentifier);

        public override void Visit(AlterTableAddTableElementStatement node)
            => ValidateConstraints(node.Definition.TableConstraints, node.SchemaObjectName.BaseIdentifier);

        private static bool IsColumnNullabilityCheck(BooleanExpression predicate)
        {
            while (predicate is BooleanParenthesisExpression pe)
            {
                predicate = pe.Expression;
            }

            if (predicate is BooleanNotExpression n)
            {
                return IsColumnNullabilityCheck(n.Expression);
            }

            if (predicate is BooleanIsNullExpression isnull)
            {
                return IsColumnReference(isnull.Expression);
            }

            return false;
        }

        private static bool IsColumnReference(ScalarExpression expr)
        {
            while (expr is ParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            if (expr is ColumnReferenceExpression)
            {
                return true;
            }

            if (expr is FunctionCall fn && fn.FunctionName.Value.Equals("ISNULL", StringComparison.OrdinalIgnoreCase))
            {
                return fn.Parameters.Count == 2
                    && fn.Parameters[1] is Literal
                    && IsColumnReference(fn.Parameters[0]);
            }

            return false;
        }

        private void ValidateConstraints(IList<ConstraintDefinition> constraints, Identifier parentId)
        {
            if (constraints is null || constraints.Count == 0)
            {
                return;
            }

            int n = constraints.Count;
            for (int i = 0; i < n; i++)
            {
                if (constraints[i] is CheckConstraintDefinition check
                && IsColumnNullabilityCheck(check.CheckCondition))
                {
                    HandleNodeError(check, check.ConstraintIdentifier?.Value ?? parentId?.Value);
                }
            }
        }
    }
}
