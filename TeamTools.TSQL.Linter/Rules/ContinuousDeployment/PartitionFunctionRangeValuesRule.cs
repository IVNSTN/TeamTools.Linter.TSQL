using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    // See also PartitionRangeListRule
    [RuleIdentity("CD0833", "PARTITIONING_RANGE_VALUES")]
    internal sealed class PartitionFunctionRangeValuesRule : AbstractRule
    {
        public PartitionFunctionRangeValuesRule() : base()
        {
        }

        public override void ExplicitVisit(CreatePartitionFunctionStatement node)
        {
            if (node.BoundaryValues.Count > 1)
            {
                HandleNodeError(node.BoundaryValues[1], node.BoundaryValues.Count.ToString());
            }
        }
    }
}
