using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : collapse all similar _IN_CONSTRAINT rules into one, rename with respect to computed columns
    [RuleIdentity("CD0216", "CAST_IN_CONSTRAINT")]
    internal sealed class CastInConstraintRule : AbstractRule
    {
        public CastInConstraintRule() : base()
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
            var cstrVisitor = new ConstraintDefinitionValidator(() => new CastVisitor(), HandleNodeError);
            node.AcceptChildren(cstrVisitor);

            var computeVisitor = new ComputedColumnValidator(() => new CastVisitor(), HandleNodeError);
            node.AcceptChildren(computeVisitor);
        }

        private class CastVisitor : TSqlViolationDetector
        {
            public override void Visit(CastCall node) => MarkDetected(node);
        }
    }
}
