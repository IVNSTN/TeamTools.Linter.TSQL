using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// Main SqlScriptAnalyzer class definition with all the ScalarExpression processing.
    /// </summary>
    public partial class SqlScriptAnalyzer : TSqlFragmentVisitor
    {
        private readonly IVariableEvaluatedValueRegistry varRegistry;
        private readonly IExpressionEvaluator evaluator;
        private readonly ISqlTypeResolver typeResolver;
        private readonly ISqlTypeConverter converter;
        private readonly IConditionalFlowHandler conditionHandler;
        private readonly IViolationRegistrar violations;
        private readonly WalkThroughProtector walkThrough;

        public SqlScriptAnalyzer(
            IVariableEvaluatedValueRegistry varRegistry,
            IExpressionEvaluator evaluator,
            ISqlTypeResolver typeResolver,
            ISqlTypeConverter converter,
            IConditionalFlowHandler conditionHandler,
            IViolationRegistrar violations)
        {
            this.varRegistry = varRegistry ?? throw new ArgumentNullException(nameof(varRegistry));
            this.evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            this.typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
            this.converter = converter ?? throw new ArgumentNullException(nameof(converter));
            this.conditionHandler = conditionHandler ?? throw new ArgumentNullException(nameof(conditionHandler));
            this.violations = violations ?? throw new ArgumentNullException(nameof(violations));

            walkThrough = new WalkThroughProtector();
        }

        public IVariableEvaluatedValueRegistry VarRegistry => varRegistry;

        public IExpressionEvaluator Evaluator => evaluator;

        public ISqlTypeResolver TypeResolver => typeResolver;

        public ISqlTypeConverter Converter => converter;

        public IConditionalFlowHandler ConditionHandler => conditionHandler;

        // Validating all scalar expressions even if they are not involved in
        // variable assignments. This includes built-in function calls.
        public override void Visit(ScalarExpression node)
        {
            walkThrough.Run(node, () =>
            {
                evaluator.EvaluateExpression(node);
            });
        }
    }
}
