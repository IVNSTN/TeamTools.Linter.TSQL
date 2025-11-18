using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class Iif : CaseBasedResultFunctionHandler
    {
        private static readonly int RequiredArgumentCount = 3;
        private static readonly string FuncName = "IIF";

        public Iif() : base(FuncName, RequiredArgumentCount, RequiredArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<CaseArgs> call)
        {
            // TODO : and predicate
            return ValidationScenario
                    .For("THEN", call.RawArgs[1], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .Then(v => call.ValidatedArgs.Values.Add(v))
                && ValidationScenario
                    .For("ELSE", call.RawArgs[2], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .Then(v => call.ValidatedArgs.Values.Add(v));
        }

        // TODO : detect always true / false predicate
        protected override SqlValue EvaluateValuesToSpecificResult(List<SqlValue> values, EvaluationContext context) => default;
    }
}
