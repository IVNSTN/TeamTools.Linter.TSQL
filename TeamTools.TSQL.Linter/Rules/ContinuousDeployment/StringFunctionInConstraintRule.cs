using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : collapse all similar _IN_CONSTRAINT rules into one
    [RuleIdentity("CD0282", "STRING_FN_IN_CONSTRAINT")]
    internal sealed class StringFunctionInConstraintRule : AbstractRule
    {
        public StringFunctionInConstraintRule() : base()
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
            var cstrVisitor = new ConstraintDefinitionValidator(() => new StringFunctionVisitor(), HandleNodeError);
            node.AcceptChildren(cstrVisitor);
        }

        private class StringFunctionVisitor : TSqlViolationDetector
        {
            public override void Visit(LeftFunctionCall node) => MarkDetected(node);

            public override void Visit(RightFunctionCall node) => MarkDetected(node);
        }
    }
}
