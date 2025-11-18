using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// RedundantNestedConditionRule implementation.
    /// </summary>
    [RuleIdentity("RD0782", "REDUNDANT_NESTED_CONDITION")]
    internal sealed partial class RedundantNestedConditionRule : AbstractRule
    {
        public RedundantNestedConditionRule() : base()
        {
        }

        public override void Visit(IfStatement node) => Proceed(node.Predicate, node.ThenStatement);

        public override void Visit(WhileStatement node) => Proceed(node.Predicate, node.Statement);

        public override void Visit(SearchedWhenClause node) => Proceed(node.WhenExpression, node.ThenExpression);

        public override void Visit(SimpleCaseExpression node)
        {
            int n = node.WhenClauses.Count;
            for (int i = 0; i < n; i++)
            {
                var option = node.WhenClauses[i];

                var predicate = new BooleanSurrogateExpression
                {
                    ComparisonType = BooleanComparisonType.Equals,
                    FirstExpression = node.InputExpression,
                    SecondExpression = option.WhenExpression,
                };

                Proceed(predicate, option.ThenExpression);
            }
        }

        public override void Visit(IIfCall node) => Proceed(node.Predicate, node.ThenExpression);

        public override void Visit(QualifiedJoin node) => Proceed(node.SearchCondition, default);

        public override void Visit(WhereClause node) => Proceed(node.SearchCondition, default);

        private void Proceed(BooleanExpression outerPredicate, TSqlFragment innerStatements)
        {
            if (outerPredicate is null)
            {
                return;
            }

            new DiveDeepPredicate(outerPredicate, innerStatements, ViolationHandlerWithMessage).Run();
        }
    }
}
