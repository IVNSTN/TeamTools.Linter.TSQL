using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("MA0116", "ERROR_SWALLOW")]
    internal sealed class ErrorSwallowingRule : AbstractRule
    {
        public ErrorSwallowingRule() : base()
        {
        }

        public override void Visit(TryCatchStatement node)
        {
            if (node.CatchStatements.Statements.Any(st => !(st is TransactionStatement || st is SetOnOffStatement)))
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
