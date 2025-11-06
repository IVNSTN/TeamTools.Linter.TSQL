using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : collapse all similar _IN_CONSTRAINT rules into one, rename with respect to computed columns
    [RuleIdentity("CD0214", "LIST_IN_CONSTRAINT")]
    internal sealed class ConstraintWithoutListsRule : AbstractRule
    {
        public ConstraintWithoutListsRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node) => DoValidate(node.SchemaObjectName, node);

        public override void Visit(AlterTableStatement node) => DoValidate(node.SchemaObjectName, node);

        public override void Visit(CreateTypeTableStatement node) => DoValidate(node);

        private void DoValidate(SchemaObjectName name, TSqlFragment node)
        {
            if (name.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                // ignoring #
                return;
            }

            DoValidate(node);
        }

        private void DoValidate(TSqlFragment node)
        {
            var cstrVisitor = new ConstraintDefinitionValidator(() => new InDetector(), HandleNodeError);
            node.AcceptChildren(cstrVisitor);

            var computeVisitor = new ComputedColumnValidator(() => new InDetector(), HandleNodeError);
            node.AcceptChildren(computeVisitor);
        }

        private class InDetector : TSqlViolationDetector
        {
            public override void Visit(InPredicate node) => MarkDetected(node);
        }
    }
}
