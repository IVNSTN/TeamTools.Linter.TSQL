using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class BaseTemporalTableRangeValidator
    {
        protected static DefaultConstraintDefinition ExtractDefaultConstraint(ColumnDefinition col)
        {
            if (col.DefaultConstraint != null)
            {
                return col.DefaultConstraint;
            }

            for (int i = col.Constraints.Count - 1; i >= 0; i--)
            {
                if (col.Constraints[i] is DefaultConstraintDefinition d)
                {
                    return d;
                }
            }

            return default;
        }

        protected static ScalarExpression ExpandExpression(ScalarExpression expr)
        {
            while (expr is ParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            return expr;
        }

        [ExcludeFromCodeCoverage]
        protected virtual bool IsDefaultAlright(DefaultConstraintDefinition def)
        {
            return true;
        }

        protected virtual bool IsDefaultAlright(ColumnDefinition col)
        {
            return IsDefaultAlright(ExtractDefaultConstraint(col));
        }
    }
}
