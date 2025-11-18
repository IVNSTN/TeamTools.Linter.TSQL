using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines
{
    internal sealed class ComputedColumnValidator : TSqlFragmentVisitor
    {
        private readonly Func<TSqlViolationDetector> validatorFactoryMethod;
        private readonly Action<TSqlFragment> callback;

        public ComputedColumnValidator(Func<TSqlViolationDetector> validatorFactoryMethod, Action<TSqlFragment> callback)
        {
            this.validatorFactoryMethod = validatorFactoryMethod;
            this.callback = callback;
        }

        public override void Visit(ColumnDefinition node)
            => TSqlViolationDetector.DetectFirst(validatorFactoryMethod(), node.ComputedColumnExpression, callback);
    }
}
