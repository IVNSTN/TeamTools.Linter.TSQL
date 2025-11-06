using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("MA0167", "COMPUTED_OUTPUT_NO_ALIAS")]
    internal sealed class ComputedColumnOutputAliasRule : AbstractRule
    {
        public ComputedColumnOutputAliasRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            /* TODO : (select 1) as t (value INT) */

            var selectElementVisitor = new SelectElementIdentifierVisitor(true, (col) =>
            {
                if (col.ColumnName != null)
                {
                    return;
                }

                if (col.Expression is ColumnReferenceExpression colRef && colRef.MultiPartIdentifier?.Count > 0)
                {
                    return;
                }

                HandleNodeError(col);
            });

            node.AcceptChildren(selectElementVisitor);
        }
    }
}
