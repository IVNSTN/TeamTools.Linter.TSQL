using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0107", "TMP_NAMED_CONSTRAINT")]
    internal sealed class ConstraintNameInTempTableRule : AbstractRule
    {
        public ConstraintNameInTempTableRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            // only apply rule to temp tables
            if (!node.SchemaObjectName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                return;
            }

            var constraintVisitor = new ConstraintVisitor();
            node.AcceptChildren(constraintVisitor);

            if (constraintVisitor.NamedConstraintExists)
            {
                HandleNodeError(node);
            }
        }

        private class ConstraintVisitor : TSqlFragmentVisitor
        {
            public bool NamedConstraintExists
            {
                get;
                private set;
            }

            public override void Visit(ConstraintDefinition node)
            {
                if (NamedConstraintExists)
                {
                    return;
                }

                NamedConstraintExists = node.ConstraintIdentifier != null;
            }
        }
    }
}
