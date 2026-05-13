using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0876", "EXPAND_DATE_FUNC")]
    internal sealed partial class ExpandDateFunctionRule : AbstractRule
    {
        private readonly TSqlFragmentVisitor badPredicateDetector;

        public ExpandDateFunctionRule() : base()
        {
            badPredicateDetector = new BadPredicateDetector(ViolationHandlerWithMessage);
        }

        public override void Visit(QualifiedJoin node) => node.SearchCondition.Accept(badPredicateDetector);

        public override void Visit(WhereClause node) => node.SearchCondition?.Accept(badPredicateDetector);
    }
}
