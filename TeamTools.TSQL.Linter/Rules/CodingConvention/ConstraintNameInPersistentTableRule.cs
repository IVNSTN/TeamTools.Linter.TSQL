using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0117", "UNNAMED_CONSTRAINT")]
    internal sealed class ConstraintNameInPersistentTableRule : AbstractRule
    {
        public ConstraintNameInPersistentTableRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            // only apply rule to non-temp tables
            if (node.SchemaObjectName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                return;
            }

            node.AcceptChildren(new ConstraintVisitor(HandleNodeError));
        }

        private class ConstraintVisitor : VisitorWithCallback
        {
            public ConstraintVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(ConstraintDefinition node)
            {
                if (!(node is NullableConstraintDefinition)
                && node.ConstraintIdentifier is null)
                {
                    Callback(node);
                }
            }
        }
    }
}
