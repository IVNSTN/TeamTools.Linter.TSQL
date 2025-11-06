using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
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
                ignoredNode = new ColumnReferenceExpression
                {
                    FirstTokenIndex = query.SelectElements.First().FirstTokenIndex,
                    LastTokenIndex = query.SelectElements.Last().LastTokenIndex,
                };
            }

            walkThrough.Run(ignoredNode, () =>
            {
                // just mark it as already analyzed so all the scalar
                // expressions from it are ignored in other analysis methods
            });
        }
    }
}
