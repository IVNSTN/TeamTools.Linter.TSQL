using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines
{
    [Obsolete("Rewrite code without using this class. Take recent IifInConstraintRule class changes as example.")]
    internal sealed class ConstraintDefinitionValidator : TSqlFragmentVisitor
    {
        private readonly Func<TSqlViolationDetector> validatorFactoryMethod;
        private readonly Action<TSqlFragment> callback;

        public ConstraintDefinitionValidator(Func<TSqlViolationDetector> validatorFactoryMethod, Action<TSqlFragment> callback)
        {
            this.validatorFactoryMethod = validatorFactoryMethod;
            this.callback = callback;
        }

        public override void Visit(DefaultConstraintDefinition node) => DoValidate(node);

        public override void Visit(CheckConstraintDefinition node) => DoValidate(node);

        private void DoValidate(TSqlFragment node)
            => TSqlViolationDetector.DetectFirst(validatorFactoryMethod(), node, nd => callback(nd ?? node));
    }
}
