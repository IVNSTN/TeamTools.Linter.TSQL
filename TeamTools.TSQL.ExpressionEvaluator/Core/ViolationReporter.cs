using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.ExpressionEvaluator.Core
{
    public class ViolationReporter : IViolationRegistrar, IViolationReporter
    {
        public int ViolationCount => Violations.Count;

        public List<SqlViolation> Violations { get; } = new List<SqlViolation>();

        public void RegisterViolation(SqlViolation violation)
            => Violations.Add(violation ?? throw new ArgumentNullException(nameof(violation)));
    }
}
