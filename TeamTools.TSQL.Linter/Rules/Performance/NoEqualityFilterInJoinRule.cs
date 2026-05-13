using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0880", "NO_EQUALITY_FILTER_JOIN")]
    internal sealed class NoEqualityFilterInJoinRule : BaseNoEqualityFilterRule
    {
        public NoEqualityFilterInJoinRule() : base()
        {
        }

        public override void Visit(QualifiedJoin node) => ValidatePredicate(node.SearchCondition);
    }
}
