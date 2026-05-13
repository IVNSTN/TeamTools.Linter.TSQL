using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0107", "TMP_NAMED_CONSTRAINT")]
    internal sealed class ConstraintNameInTempTableRule : AbstractRule
    {
        private readonly ConstraintVisitor constraintVisitor;

        public ConstraintNameInTempTableRule() : base()
        {
            constraintVisitor = new ConstraintVisitor(ViolationHandler);
        }

        public override void Visit(CreateTableStatement node)
        {
            // only apply rule to temp tables
            if (!IsTempTable(node.SchemaObjectName))
            {
                return;
            }

            node.Definition.AcceptChildren(constraintVisitor);
        }

        public override void Visit(AlterTableAddTableElementStatement node)
        {
            // only apply rule to temp tables
            if (!IsTempTable(node.SchemaObjectName))
            {
                return;
            }

            node.Definition.AcceptChildren(constraintVisitor);
        }

        private static bool IsTempTable(SchemaObjectName name)
        {
            return name.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix);
        }

        private sealed class ConstraintVisitor : VisitorWithCallback
        {
            public ConstraintVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(ConstraintDefinition node)
            {
                if (node.ConstraintIdentifier != null)
                {
                    Callback(node);
                }
            }
        }
    }
}
