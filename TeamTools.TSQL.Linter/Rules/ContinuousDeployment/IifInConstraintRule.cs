using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : collapse all similar _IN_CONSTRAINT rules into one, rename with respect to computed columns
    [RuleIdentity("CD0280", "IIF_IN_CONSTRAINT")]
    [CompatibilityLevel(SqlVersion.Sql110)]
    internal sealed class IifInConstraintRule : AbstractRule
    {
        public IifInConstraintRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node) => DoValidate(node.SchemaObjectName, node);

        public override void Visit(AlterTableStatement node) => DoValidate(node.SchemaObjectName, node);

        public override void Visit(CreateTypeTableStatement node) => DoValidate(node);

        private void DoValidate(SchemaObjectName name, TSqlFragment body)
        {
            if (name.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                // ignoring #
                return;
            }

            DoValidate(body);
        }

        private void DoValidate(TSqlFragment node)
        {
            var cstrVisitor = new ConstraintDefinitionValidator(() => new IifFunctionVisitor(), HandleNodeError);
            node.AcceptChildren(cstrVisitor);

            var computeVisitor = new ComputedColumnValidator(() => new IifFunctionVisitor(), HandleNodeError);
            node.AcceptChildren(computeVisitor);
        }

        private class IifFunctionVisitor : TSqlViolationDetector
        {
            public override void Visit(IIfCall node) => MarkDetected(node);
        }
    }
}
