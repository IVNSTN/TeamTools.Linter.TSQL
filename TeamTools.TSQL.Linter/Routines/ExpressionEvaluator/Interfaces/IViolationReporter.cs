using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public interface IViolationReporter
    {
        IEnumerable<SqlViolation> Violations { get; }
    }
}
