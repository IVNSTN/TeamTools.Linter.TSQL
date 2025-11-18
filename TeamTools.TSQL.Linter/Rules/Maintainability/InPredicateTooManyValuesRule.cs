using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("MA0816", "IN_VALUES_TOO_MANY")]
    internal sealed class InPredicateTooManyValuesRule : AbstractRule
    {
        private static readonly int MaxAlowedValues = 12;

        public InPredicateTooManyValuesRule() : base()
        {
        }

        public override void Visit(InPredicate node)
        {
            if (node.Subquery != null)
            {
                return;
            }

            int n = node.Values.Count;
            if (n > MaxAlowedValues)
            {
                HandleNodeError(node.Values[n - 1], n.ToString());
            }
        }
    }
}
