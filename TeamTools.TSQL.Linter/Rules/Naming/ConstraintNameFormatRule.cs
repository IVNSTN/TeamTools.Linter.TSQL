using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Constraint name validator.
    /// </summary>
    [RuleIdentity("NM0203", "CONSTRAINT_NAME_PATTERN")]
    internal sealed partial class ConstraintNameFormatRule : AbstractRule
    {
        public ConstraintNameFormatRule() : base()
        {
        }

        private enum ConstraintType
        {
            PrimaryKey,
            Unique,
            Check,
            Default,
            ForeignKey,
        }

        public override void Visit(CreateTableStatement node) => ValidateAllConstraints(node, node.SchemaObjectName);

        // we need both ADD COLUMN and ADD CONSTRAINT statements
        // thus catching closest common ancestor - AlterTableStatement
        public override void Visit(AlterTableStatement node) => ValidateAllConstraints(node, node.SchemaObjectName);

        private void ValidateAllConstraints(TSqlFragment node, SchemaObjectName tableName)
        {
            if (tableName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                // only apply rule to non-temp tables
                // there is a separate rule which forbids giving names to temp table constraints
                return;
            }

            var constraintVisitor = new ConstraintVisitor(tableName, null, new ConstraintNameBuilder(), HandleNodeError);
            var columnVisitor = new TableColumnVisitor(tableName, new ConstraintNameBuilder(), HandleNodeError);
            // looking for constraints defined at table level
            node.AcceptChildren(constraintVisitor);
            // looking for constraints defined inline - in column definition
            node.AcceptChildren(columnVisitor);
        }
    }
}
