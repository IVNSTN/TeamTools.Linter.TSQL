using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0171", "COLUMN_FULLY_QUALIFIED")]
    internal sealed class ColumnFullQualifiedReferenceRule : AbstractRule
    {
        public ColumnFullQualifiedReferenceRule() : base()
        {
        }

        public override void Visit(ColumnReferenceExpression node)
        {
            if (null == node.MultiPartIdentifier || node.MultiPartIdentifier.Count <= 2)
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
