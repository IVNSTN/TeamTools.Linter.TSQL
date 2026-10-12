using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Ambiguous column reference detector.
    /// </summary>
    [RuleIdentity("AM0935", "AMBIGUOUS_COL_SOURCE")]
    internal sealed partial class AmbiguousColumnBelongingRule : AbstractRule
    {
        public AmbiguousColumnBelongingRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            node.AcceptChildren(new QueryValidator(ViolationHandlerWithMessage));
        }
    }
}
