using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public class ViolationReporter : IViolationRegistrar, IViolationReporter
    {
        private readonly ICollection<SqlViolation> violations = new List<SqlViolation>(100);

        public int ViolationCount => violations.Count;

        public IEnumerable<SqlViolation> Violations => violations;

        public void RegisterViolation(SqlViolation violation)
            => violations.Add(violation ?? throw new ArgumentNullException(nameof(violation)));
    }
}
