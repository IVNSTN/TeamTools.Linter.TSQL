using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions
{
    public abstract class ObjectIdentificationFunction : SqlGenericFunctionHandler<ObjectIdentificationFunction.ObjIdArgs>
    {
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.SysName;

        protected ObjectIdentificationFunction(string funcName, int minArgs, int maxArgs)
        : base(funcName, minArgs, maxArgs)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<ObjIdArgs> call)
        {
            return (ValidationScenario
                    .For("OBJECT_ID", call.RawArgs[0], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidInt.Validate)
                    .Then(i => call.ValidatedArgs.ObjectId = i)
                && (call.RawArgs.Count < 2
                || ValidationScenario
                    .For("DB_ID", call.RawArgs[1], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidInt.Validate)
                    .Then(i => call.ValidatedArgs.DatabaseId = i)))
                // if object_id could not be parsed we still can estimate
                // to approximate value
                || true;
        }

        protected override string DoEvaluateResultType(CallSignature<ObjIdArgs> call) => ResultTypeName;

        protected override SqlValue DoEvaluateResultValue(CallSignature<ObjIdArgs> call)
        {
            var res = call.Context.Converter.ImplicitlyConvert<SqlStrTypeValue>(base.DoEvaluateResultValue(call));

            if (call.ValidatedArgs.DatabaseId != null && call.ValidatedArgs.DatabaseId is CurrentDatabaseId)
            {
                call.Context.RedundantArgument("DB_ID", "Current DB is the default context");
            }

            if (call.ValidatedArgs.ObjectId is null)
            {
                // Could not evaluate source value
                // but result size is fixed
                return res;
            }

            if (call.ValidatedArgs.ObjectId.IsNull)
            {
                call.Context.RedundantCall("Object ID is NULL");

                return res.TypeReference.MakeNullValue();
            }

            if (call.ValidatedArgs.ObjectId is CurrentProcReference procRef)
            {
                return res.TypeHandler.StrValueFactory.MakePreciseValue(
                    call.ResultType,
                    GetNamePartForCurrentProc(procRef),
                    res.Source);
            }

            return res;
        }

        protected virtual string GetNamePartForCurrentProc(CurrentProcReference currentProc)
            => currentProc.ProcName;

        public class ObjIdArgs
        {
            public SqlIntTypeValue ObjectId { get; set; }

            public SqlIntTypeValue DatabaseId { get; set; }
        }
    }
}
