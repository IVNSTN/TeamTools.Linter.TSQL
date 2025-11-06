using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class DbId : SqlGenericFunctionHandler<DbId.DbIdArgs>
    {
        private static readonly string FuncName = "DB_ID";
        private static readonly string ResultTypeName = "dbo.INT";
        private static readonly int MinArgCount = 0;
        private static readonly int MaxArgCount = 1;
        private static readonly SqlIntValueRange DbIdRange = new SqlIntValueRange(0, int.MaxValue);

        public DbId() : base(FuncName, MinArgCount, MaxArgCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<DbIdArgs> call)
        {
            if (call.RawArgs.Count > 0)
            {
                ValidationScenario
                    .For("DATABASE_NAME", call.RawArgs[0], call.Context)
                    .When<SqlValue>(ArgumentIsValue.Validate)
                    .And<SqlStrTypeValue>(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.DatabaseName = s);
            }

            return true;
        }

        protected override string DoEvaluateResultType(CallSignature<DbIdArgs> call) => ResultTypeName;

        protected override SqlValue DoEvaluateResultValue(CallSignature<DbIdArgs> call)
        {
            var value = call.Context.Converter
                .ImplicitlyConvert<SqlIntTypeValue>(base.DoEvaluateResultValue(call))?
                .ChangeTo(DbIdRange, call.Context.NewSource);

            if (value is null)
            {
                return default;
            }

            if (call.RawArgs.Count == 0)
            {
                return new CurrentDatabaseId(value.TypeHandler, call.Context.NewSource);
            }

            if (call.ValidatedArgs.DatabaseName.IsNull)
            {
                call.Context.RedundantCall("DB name is NULL");

                return value.TypeHandler.IntValueFactory.NewNull(call.Context.Node);
            }

            return value.ChangeTo(DbIdRange, value.Source);
        }

        public class DbIdArgs
        {
            public SqlStrTypeValue DatabaseName { get; set; }
        }
    }
}
