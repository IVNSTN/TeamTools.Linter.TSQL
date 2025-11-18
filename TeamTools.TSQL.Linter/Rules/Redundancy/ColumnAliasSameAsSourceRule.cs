using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0713", "COLUMN_ALIAS_IS_THE_SAME")]
    internal sealed class ColumnAliasSameAsSourceRule : AbstractRule
    {
        public ColumnAliasSameAsSourceRule() : base()
        {
        }

        public override void Visit(SelectScalarExpression node)
        {
            if (node.ColumnName is null || !(node.Expression is ColumnReferenceExpression colRef)
            || colRef.MultiPartIdentifier is null)
            {
                // no alias or this is a computed column which needs alias
                // or source is a pseudo column, e.h. $action
                return;
            }

            if (!string.Equals(colRef.MultiPartIdentifier.GetLastIdentifier().Value, node.ColumnName.Value))
            {
                // not the same
                return;
            }

            HandleNodeError(node.ColumnName, node.ColumnName.Value);
        }
    }
}
