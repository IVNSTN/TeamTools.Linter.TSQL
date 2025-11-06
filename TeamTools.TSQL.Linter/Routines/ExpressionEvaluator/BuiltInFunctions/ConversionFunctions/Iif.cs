using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
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
                    .When<SqlValue>(ArgumentIsValue.Validate)
                    .Then(v => call.ValidatedArgs.Values.Add(v))
                && ValidationScenario
                    .For("ELSE", call.RawArgs[2], call.Context)
                    .When<SqlValue>(ArgumentIsValue.Validate)
                    .Then(v => call.ValidatedArgs.Values.Add(v));
        }

        // TODO : detect always true / false predicate
        protected override SqlValue EvaluateValuesToSpecificResult(IList<SqlValue> values, EvaluationContext context) => default;
    }
}
