using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("MA0167", "COMPUTED_OUTPUT_NO_ALIAS")]
    internal sealed class ComputedColumnOutputAliasRule : AbstractRule
    {
        private readonly Action<SelectScalarExpression> validator;

        public ComputedColumnOutputAliasRule() : base()
        {
            validator = new Action<SelectScalarExpression>(ValidateColumnAlias);
        }

        protected override void ValidateScript(TSqlScript node) => node.AcceptChildren(new SelectElementIdentifierVisitor(validator));

        private void ValidateColumnAlias(SelectScalarExpression col)
        {
            if (col.ColumnName != null)
            {
                // it has a name
                return;
            }

            if (col.Expression is ColumnReferenceExpression colRef && colRef.MultiPartIdentifier?.Count > 0)
            {
                return;
            }

            HandleNodeError(col);
        }
    }
}
