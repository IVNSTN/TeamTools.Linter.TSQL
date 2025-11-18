using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// Scalar expressions from EXISTS SELECT part should not raise any violations
    /// because database engine ignores the SELECT part in EXISTS.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        public override void Visit(ExistsPredicate node)
        {
            TSqlFragment ignoredNode;

            var query = node.Subquery.QueryExpression.GetQuerySpecification();
            if (query is null)
            {
                ignoredNode = node;
            }
            else
            {
                // Just some TSQLFragment instance with boundaries
                // Note, only SELECT part is supposed to be ignored here.
                // WHERE predicate expressions and others will be analyzed
                // in a regular manner.
                ignoredNode = new ColumnReferenceExpression
                {
                    FirstTokenIndex = query.SelectElements[0].FirstTokenIndex,
                    LastTokenIndex = query.SelectElements[query.SelectElements.Count - 1].LastTokenIndex,
                };
            }

            // Just mark it as "already analyzed" so all the scalar
            // expressions from it are ignored in other analysis methods.
            walkThrough.Run(ignoredNode);
        }
    }
}
