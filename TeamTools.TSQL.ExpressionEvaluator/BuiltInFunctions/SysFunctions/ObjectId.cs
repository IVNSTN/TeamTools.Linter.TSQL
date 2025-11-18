using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ObjectId : SqlGenericFunctionHandler<ObjectId.ObjectIdArgs>
    {
        private static readonly string FuncName = "OBJECT_ID";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.Int;
        private static readonly int MinArgCount = 1;
        private static readonly int MaxArgCount = 2;
        private static readonly SqlIntValueRange ObjectIdRange = new SqlIntValueRange(0, int.MaxValue);
        private static readonly HashSet<string> SupportedObjectTypes;

        // TODO : consolidate all the metadata in resource file
        static ObjectId()
        {
            SupportedObjectTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "AF",
                "C",
                "D",
                "F",
                "FN",
                "FS",
                "FT",
                "IF",
                "IT",
                "P",
                "PC",
                "PG",
                "PK",
                "R",
                "RF",
                "S",
                "SN",
                "SO",
                "U",
                "V",
                "SQ",
                "TA",
                "TF",
                "TR",
                "TT",
                "UQ",
                "X",
                "ST",
                "ET",
                "EC",
            };
        }

        public ObjectId() : base(FuncName, MinArgCount, MaxArgCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<ObjectIdArgs> call)
        {
            return ValidationScenario
                .For("OBJECT_NAME", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidStr.Validate)
                .Then(s => call.ValidatedArgs.ObjectName = s)
            && (call.RawArgs.Count < 2
            || ValidationScenario
                .For("OBJECT_TYPE", call.RawArgs[1], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidStr.Validate)
                .Then(s => call.ValidatedArgs.ObjectType = s));
        }

        protected override string DoEvaluateResultType(CallSignature<ObjectIdArgs> call) => ResultTypeName;

        // TODO : validate ObjectType precise value
        protected override SqlValue DoEvaluateResultValue(CallSignature<ObjectIdArgs> call)
        {
            var res = call.Context.Converter
                .ImplicitlyConvert<SqlIntTypeValue>(base.DoEvaluateResultValue(call))?
                .ChangeTo(ObjectIdRange, call.Context.NewSource);

            if (call.ValidatedArgs.ObjectName.IsNull || call.ValidatedArgs.ObjectName.EstimatedSize == 0)
            {
                call.Context.RedundantCall("Object name is NULL or empty");
                return res.TypeReference.MakeNullValue();
            }

            if (call.ValidatedArgs.ObjectType != null
            && (call.ValidatedArgs.ObjectType.IsNull || call.ValidatedArgs.ObjectType.EstimatedSize == 0))
            {
                call.Context.RedundantCall("Object type is NULL or empty");
                return res.TypeReference.MakeNullValue();
            }

            if (call.ValidatedArgs.ObjectType != null && call.ValidatedArgs.ObjectType.IsPreciseValue
            && !SupportedObjectTypes.Contains(call.ValidatedArgs.ObjectType.Value))
            {
                call.Context.InvalidArgument("OBJECT_TYPE", $"'{call.ValidatedArgs.ObjectType.Value}' is not supported object type");
            }

            return res;
        }

        public class ObjectIdArgs
        {
            public SqlStrTypeValue ObjectName { get; set; }

            public SqlStrTypeValue ObjectType { get; set; }
        }
    }
}
