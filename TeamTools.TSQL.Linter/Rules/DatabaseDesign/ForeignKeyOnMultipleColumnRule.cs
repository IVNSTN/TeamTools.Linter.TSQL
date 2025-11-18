using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0153", "FK_MULTIPLE_COL")]
    internal sealed class ForeignKeyOnMultipleColumnRule : AbstractRule
    {
        public ForeignKeyOnMultipleColumnRule() : base()
        {
        }

        public override void Visit(ForeignKeyConstraintDefinition node)
        {
            if (node.ReferencedTableColumns.Count == 1)
            {
                return;
            }

            HandleNodeError(node, node.ReferencedTableColumns.Count.ToString());
        }
    }
}
