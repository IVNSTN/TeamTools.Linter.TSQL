using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Script analyzer.
    /// </summary>
    [RuleIdentity("FA0957", "NUMERIC_OPERATION_ON_NON_NUMERIC")]
    internal sealed partial class InvalidTypeForNumericOperationRule : AbstractRule
    {
        public InvalidTypeForNumericOperationRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var evaluator = new ExpressionResultTypeEvaluator(node)
                .InjectKnownReturnTypes(knownReturnTypes);

            // TODO : ExpressionValidator can be "singleton" it only needs to be recreated because of ExpressionResultTypeEvaluator
            var validator = new ExpressionValidator(KnownTypes, evaluator, ViolationHandlerWithMessage);
            node.AcceptChildren(validator);
        }
    }
}
