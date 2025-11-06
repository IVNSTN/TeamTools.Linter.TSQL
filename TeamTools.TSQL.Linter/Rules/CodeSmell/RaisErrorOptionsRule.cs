using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0134", "RAISERROR_OPTIONS")]
    internal sealed class RaisErrorOptionsRule : AbstractRule
    {
        public RaisErrorOptionsRule() : base()
        {
        }

        public override void Visit(RaiseErrorStatement node)
        {
            if ((node.RaiseErrorOptions & RaiseErrorOptions.Log) != 0)
            {
                HandleNodeError(node, "WITH LOG - stop spamming to the Server log");
            }

            if ((node.RaiseErrorOptions & RaiseErrorOptions.SetError) != 0)
            {
                HandleNodeError(node, "SETERROR - is of no use");
            }
        }
    }
}
