using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Query validation for ImplicitConversionImpossibleRule.
    /// </summary>
    internal partial class ImpossibleVisitor
    {
        public override void Visit(BinaryQueryExpression node)
        {
            var specA = node.FirstQueryExpression.GetQuerySpecification();
            var specB = node.SecondQueryExpression.GetQuerySpecification(); // TODO : better support multiple nesting

            if (specA is null || specB is null)
            {
                return;
            }

            ValidateSpecificationCompatibility(specA, specB);
        }

        private void ValidateSpecificationCompatibility(QuerySpecification specA, QuerySpecification specB)
        {
            // required equality is handled by a separate rule
            int n = specA.SelectElements.Count > specB.SelectElements.Count ? specB.SelectElements.Count : specA.SelectElements.Count;

            for (int i = 0; i < n; i++)
            {
                // TODO : grab source table name to reach to cached column types
                if (!(specA.SelectElements[i] is SelectScalarExpression exprA))
                {
                    continue;
                }

                if (!(specB.SelectElements[i] is SelectScalarExpression exprB))
                {
                    continue;
                }

                ValidateCanConvertAtoB(exprB.Expression, exprA.Expression);
            }
        }
    }
}
