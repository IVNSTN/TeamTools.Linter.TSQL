using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions
{
    public abstract class CaseBasedResultFunctionHandler : SqlGenericFunctionHandler<CaseBasedResultFunctionHandler.CaseArgs>
    {
        protected CaseBasedResultFunctionHandler(string funcName, int minArgs, int maxArgs)
        : base(funcName, minArgs, maxArgs)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<CaseArgs> call)
        {
            return AllArgsAreValues.Validate(call.RawArgs, call.Context, v => call.ValidatedArgs.Values.Add(v));
        }

        protected override SqlValue DoEvaluateResultValue(CallSignature<CaseArgs> call)
        {
            var outputEstimate = call.ValidatedArgs.Values[0];

            int n = call.ValidatedArgs.Values.Count;
            for (int i = 1; i < n; i++)
            {
                // It also finds impossible implicit conversions if any.
                // EvaluateValuesToSpecificResult is not intended to run through all arguments
                // and make an attempt to merge them all into final outputType value.
                outputEstimate = call.ResultTypeHandler.MergeTwoEstimates(outputEstimate, call.ValidatedArgs.Values[i]);
            }

            var specificResult = EvaluateValuesToSpecificResult(call.ValidatedArgs.Values, call.Context);

            return call.Context.Converter.ImplicitlyConvertTo(call.ResultType, specificResult ?? outputEstimate);
        }

        protected override string DoEvaluateResultType(CallSignature<CaseArgs> call)
            => call.Context.Converter.EvaluateOutputType(call.ValidatedArgs.Values);

        protected abstract SqlValue EvaluateValuesToSpecificResult(List<SqlValue> values, EvaluationContext context);

        public class CaseArgs
        {
            public List<SqlValue> Values { get; } = new List<SqlValue>();
        }
    }
}
