using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0806", "IDENTITY_INSERT_TEMP_TABLE")]
    internal sealed class IdentityInsertOnTempTableRule : AbstractRule
    {
        public IdentityInsertOnTempTableRule() : base()
        {
        }

        public override void Visit(SetIdentityInsertStatement node)
        {
            string tableName = node.Table.BaseIdentifier.Value;
            if (tableName.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                HandleNodeError(node, tableName);
            }
        }
    }
}
