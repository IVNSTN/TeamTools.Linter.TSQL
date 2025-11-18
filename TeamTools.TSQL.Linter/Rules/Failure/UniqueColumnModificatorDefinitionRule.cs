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

        private static IEnumerable<ColumnReferenceExpression> ExtractColumnsFromSetClauses(IList<SetClause> clauses)
        {
            int n = clauses.Count;
            for (int i = 0; i < n; i++)
            {
                if (clauses[i] is AssignmentSetClause asgn && asgn.Column != null)
                {
                    yield return asgn.Column;
                }
            }
        }

        private void ValidateSetClauses(IList<SetClause> setClauses)
        {
            if (setClauses is null || setClauses.Count <= 1)
            {
                // single or none column cannot contain dups
                return;
            }

            ValidateColumns(ExtractColumnsFromSetClauses(setClauses).ToList());
        }

        private void ValidateColumns(IList<ColumnReferenceExpression> columns)
        {
            if (columns is null || columns.Count <= 1)
            {
                // single or none column cannot contain dups
                return;
            }

            ValidateColumnNames(columns);
        }

        private void ValidateColumnNames(IList<ColumnReferenceExpression> cols)
        {
            var foundNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int n = cols.Count;
            for (int i = 0; i < n; i++)
            {
                var col = cols[i];
                if (!foundNames.Add(col.MultiPartIdentifier.GetLastIdentifier().Value))
                {
                    HandleNodeError(col);
                }
            }
        }
    }
}
