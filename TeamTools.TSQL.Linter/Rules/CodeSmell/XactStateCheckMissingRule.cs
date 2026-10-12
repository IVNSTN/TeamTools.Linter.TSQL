using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0470", "XACT_STATE_CHECK")]
    internal sealed partial class XactStateCheckMissingRule : AbstractRule
    {
        public XactStateCheckMissingRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            batch.AcceptChildren(new AfterTranVisitor(ViolationHandlerWithMessage));
        }
    }
}
