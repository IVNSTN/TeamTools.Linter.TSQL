using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0463", "SCOPE_IDENTITY_MULTI_ROW")]
    internal sealed partial class ScopeIdentityAfterMultiRowDmlRule : AbstractRule
    {
        public ScopeIdentityAfterMultiRowDmlRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            var detector = new BatchScopeIdentityDetector(ViolationHandler);
            node.Accept(detector);
        }
    }
}
