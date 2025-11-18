using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.ExpressionEvaluator.Interfaces
{
    public interface IViolationReporter
    {
        List<SqlViolation> Violations { get; }
    }
}
