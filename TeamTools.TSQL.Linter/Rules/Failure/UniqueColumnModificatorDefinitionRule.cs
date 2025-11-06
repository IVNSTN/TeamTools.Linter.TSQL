using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0901", "DUP_COLUMN_MODIFIER")]
    internal sealed class UniqueColumnModificatorDefinitionRule : AbstractRule
    {
        public UniqueColumnModificatorDefinitionRule() : base()
        {
        }

        public override void Visit(InsertSpecification node) => ValidateColumns(node.Columns);

        public override void Visit(OutputIntoClause node) => ValidateColumns(node.IntoTableColumns);

        public override void Visit(InsertMergeAction node) => ValidateColumns(node.Columns);

        public override void Visit(UpdateSpecification node) => ValidateSetClauses(node.SetClauses);

        public override void Visit(UpdateMergeAction node) => ValidateSetClauses(node.SetClauses);

        private void ValidateSetClauses(IList<SetClause> setClauses)
        {
            ExtractColumnsFromSetClauses(setClauses, out var setColumns);

            ValidateColumns(setColumns);
        }

        private void ValidateColumns(IList<ColumnReferenceExpression> columns)
        {
            if (columns is null || columns.Count == 0)
            {
                return;
            }

            ValidateColumnNames(columns);
        }

        private void ValidateColumnNames(IList<ColumnReferenceExpression> cols)
        {
            var foundNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var col in cols)
            {
                if (!foundNames.TryAddUnique(col.MultiPartIdentifier.Identifiers.Last().Value))
                {
                    HandleNodeError(col);
                }
            }
        }

        private void ExtractColumnsFromSetClauses(IList<SetClause> clauses, out IList<ColumnReferenceExpression> setColumns)
        {
            setColumns = clauses
                .OfType<AssignmentSetClause>()
                .Where(clause => clause.Column != null)
                .Select(clause => clause.Column)
                .ToList();
        }
    }
}
