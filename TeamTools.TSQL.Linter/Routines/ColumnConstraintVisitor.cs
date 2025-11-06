using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines
{
    internal sealed class ColumnConstraintVisitor : TSqlFragmentVisitor
    {
        public NullableConstraintDefinition NullConstraint { get; private set; }

        public bool NullNotNullSpecified => NullConstraint != null;

        public override void Visit(NullableConstraintDefinition node) => NullConstraint = node;
    }
}
