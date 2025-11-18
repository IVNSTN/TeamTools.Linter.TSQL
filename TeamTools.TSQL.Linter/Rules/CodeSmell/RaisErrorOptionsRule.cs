using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;

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
                HandleNodeError(node, Strings.ViolationDetails_RaisErrorOptionsRule_WithLogSpam);
            }

            if ((node.RaiseErrorOptions & RaiseErrorOptions.SetError) != 0)
            {
                HandleNodeError(node, Strings.ViolationDetails_RaisErrorOptionsRule_SetErrorIsOfNoUse);
            }
        }
    }
}
