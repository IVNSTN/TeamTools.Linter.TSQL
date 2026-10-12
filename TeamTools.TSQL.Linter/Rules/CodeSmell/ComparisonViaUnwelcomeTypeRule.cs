using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics;
using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0468", "COMPARISON_VIA_UNWELCOME_TYPE")]
    internal sealed partial class ComparisonViaUnwelcomeTypeRule : AbstractRule
    {
        public ComparisonViaUnwelcomeTypeRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            Debug.Assert(metaData != null, "metaData not set");

            if (!ScalarExpressionEvaluator.IsBatchInteresting(node))
            {
                return;
            }

            var evaluator = new ExpressionResultTypeEvaluator(node)
                .InjectKnownReturnTypes(knownReturnTypes);

            node.Accept(new ImplicitConvertionResultTypeValidator(evaluator, ViolationHandlerWithMessage));
        }
    }
}
