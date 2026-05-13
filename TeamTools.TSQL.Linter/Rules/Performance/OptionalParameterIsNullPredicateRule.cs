using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0877", "EXPAND_ISNULL_FOR_OPTIONAL_ARG")]
    internal sealed partial class OptionalParameterIsNullPredicateRule : AbstractRule
    {
        private readonly TSqlFragmentVisitor badPredicateDetector;

        public OptionalParameterIsNullPredicateRule() : base()
        {
            badPredicateDetector = new BadPredicateDetector(ViolationHandlerWithMessage);
        }

        public override void Visit(QualifiedJoin node) => node.SearchCondition.Accept(badPredicateDetector);

        public override void Visit(WhereClause node) => node.SearchCondition?.Accept(badPredicateDetector);
    }
}
