using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// BooleanSurrogateExpression class.
    /// </summary>
    internal partial class RedundantNestedConditionRule
    {
        private sealed class BooleanSurrogateExpression : BooleanComparisonExpression
        {
            // TODO : add all options
            private static readonly Dictionary<BooleanComparisonType, string> ComparisonSymbols = new Dictionary<BooleanComparisonType, string>
            {
                { BooleanComparisonType.Equals, "=" },
            };

            private string txt = null;

            public BooleanSurrogateExpression()
            { }

            public string GetSurrogateText()
            {
                return txt ?? (txt = BuildTxt());
            }

            private string BuildTxt()
            {
                var sb = ObjectPools.StringBuilderPool.Get();
                var res = sb
                    .Append(FirstExpression.GetFragmentCleanedText())
                    .Append(ComparisonSymbols[ComparisonType])
                    .Append(SecondExpression.GetFragmentCleanedText())
                    .ToString();

                ObjectPools.StringBuilderPool.Return(sb);

                return res;
            }
        }
    }
}
