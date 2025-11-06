using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

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

        public override void Visit(TSqlScript node)
        {
            var evaluator = new ExpressionResultTypeEvaluator(node);
            evaluator.InjectKnownReturnTypes(knownReturnTypes);

            var validator = new ExpressionValidator(KnownTypes, evaluator, HandleNodeError);
            node.AcceptChildren(validator);
        }
    }
}
