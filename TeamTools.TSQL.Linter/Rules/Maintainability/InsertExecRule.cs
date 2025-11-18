using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("MA0812", "INSERT_EXEC")]
    internal sealed class InsertExecRule : AbstractRule
    {
        public InsertExecRule() : base()
        {
        }

        public override void Visit(InsertSpecification node)
        {
            if (node.InsertSource is ExecuteInsertSource)
            {
                HandleNodeErrorIfAny(node.InsertSource);
            }
        }
    }
}
