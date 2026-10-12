using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Column reference visitor.
    /// </summary>
    internal partial class AmbiguousColumnBelongingRule
    {
        private sealed class ColumnRefVisitor : TSqlFragmentVisitor
        {
            public List<MultiPartIdentifier> Columns { get; } = new List<MultiPartIdentifier>();

            public override void Visit(ColumnReferenceExpression node)
            {
                if (node.MultiPartIdentifier is null)
                {
                    // e.g. $action
                    return;
                }

                Columns.Add(node.MultiPartIdentifier);
            }

            // OUTPUT INTO target column belonging is unquestionable, only source must be verified
            public override void ExplicitVisit(OutputIntoClause node)
            {
                for (int i = node.SelectColumns.Count - 1; i >= 0; i--)
                {
                    node.SelectColumns[i].Accept(this);
                }
            }

            // INSERT target column belonging is unquestionable, only source must be verified
            public override void ExplicitVisit(InsertMergeAction node) => node.Source.Accept(this);

            // In UPDATE SET all target columns belong to the target table
            // only assigned values must be validated
            public override void ExplicitVisit(AssignmentSetClause node) => node.NewValue.Accept(this);

            // ORDER BY may contain column aliases with no table alias
            // thus not validating them.
            // If there is a broken alias used in ORDER BY - another rule should detect this.
            public override void ExplicitVisit(OrderByClause node)
            { }
        }
    }
}
