using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    public abstract class CaseBasedResultFunctionHandler : SqlGenericFunctionHandler<CaseBasedResultFunctionHandler.CaseArgs>
    {
        public CaseBasedResultFunctionHandler(string funcName, int minArgs, int maxArgs)
        : base(funcName, minArgs, maxArgs)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<CaseArgs> call)
        {
            return AllArgsAreValues.Validate(call.RawArgs, call.Context, v => call.ValidatedArgs.Values.Add(v));
        }

        protected override SqlValue DoEvaluateResultValue(CallSignature<CaseArgs> call)
        {
            var outputEstimate = call.ValidatedArgs.Values.First();

            foreach (var v in call.ValidatedArgs.Values.Skip(1))
            {
                // It also finds impossible implicit conversions if any.
                // EvaluateValuesToSpecificResult is not intended to run through all arguments
                // and make an attempt to merge them all into final outputType value.
                outputEstimate = call.ResultTypeHandler.MergeTwoEstimates(outputEstimate, v);
            }

            var specificResult = EvaluateValuesToSpecificResult(call.ValidatedArgs.Values, call.Context);

            return call.Context.Converter.ImplicitlyConvertTo(call.ResultType, specificResult ?? outputEstimate);
        }

        protected override string DoEvaluateResultType(CallSignature<CaseArgs> call)
            => call.Context.Converter.EvaluateOutputType(call.ValidatedArgs.Values);

        protected abstract SqlValue EvaluateValuesToSpecificResult(IList<SqlValue> values, EvaluationContext context);

        public class CaseArgs
        {
            public IList<SqlValue> Values { get; } = new List<SqlValue>();
        }
    }
}
