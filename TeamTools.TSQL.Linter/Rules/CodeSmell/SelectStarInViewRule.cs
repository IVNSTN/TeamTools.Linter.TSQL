using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0138", "VIEW_SELECT_STAR_OUTPUT")]
    internal sealed class SelectStarInViewRule : AbstractRule
    {
        public SelectStarInViewRule() : base()
        {
        }

        public override void Visit(ViewStatementBody node) => ValidateQuery(node.SelectStatement.QueryExpression);

        private void ValidateQuery(QueryExpression query)
        {
            if (query is BinaryQueryExpression bin)
            {
                ValidateQuery(bin.FirstQueryExpression);
                ValidateQuery(bin.SecondQueryExpression);
            }
            else if (query is QueryParenthesisExpression qp)
            {
                ValidateQuery(qp.QueryExpression);
            }
            else if (query is QuerySpecification spec)
            {
                HandleNodeErrorIfAny(spec.SelectElements.OfType<SelectStarExpression>());
            }
        }
    }
}
