using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class SchemaName : ObjectIdentificationFunction
    {
        private static readonly string FuncName = "SCHEMA_NAME";
        private static readonly int MinArgCount = 0;
        private static readonly int MaxArgCount = 1;

        public SchemaName() : base(FuncName, MinArgCount, MaxArgCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<ObjIdArgs> call)
        {
            if (call.RawArgs.Count > 0)
            {
                ValidationScenario
                    .For("SCHEMA_ID", call.RawArgs[0], call.Context)
                    .When<SqlValue>(ArgumentIsValue.Validate)
                    .And<SqlIntTypeValue>((arg, src, success) => ArgumentIsValidInt.Validate(arg, src, success))
                    .Then(i => call.ValidatedArgs.ObjectId = i);
            }

            // if object_id could not be parsed we still can estimate
            // to approximate value
            return true;
        }

        protected override SqlValue DoEvaluateResultValue(CallSignature<ObjIdArgs> call)
        {
            var res = call.Context.Converter.ImplicitlyConvert<SqlStrTypeValue>(base.DoEvaluateResultValue(call));

            if (call.ValidatedArgs.ObjectId is null)
            {
                // If schema_id omitted then the default schema name will be returned
                return res.ChangeTo(TSqlDomainAttributes.DefaultSchemaName, call.Context.NewSource);
            }

            return base.DoEvaluateResultValue(call);
        }

        // TODO : Not sure if this is correct approach. @@PROCID != proc schema id
        protected override string GetNamePartForCurrentProc(CurrentProcReference currentProc)
            => currentProc.ProcSchema;
    }
}
