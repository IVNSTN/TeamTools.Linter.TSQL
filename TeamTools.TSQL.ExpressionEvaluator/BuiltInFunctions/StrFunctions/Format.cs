using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    // https://learn.microsoft.com/en-us/sql/t-sql/functions/format-transact-sql
    public class Format : SqlGenericFunctionHandler<Format.FormatArgs>
    {
        private static readonly string FuncName = "FORMAT";
        private static readonly string ResultType = TSqlDomainAttributes.Types.NVarchar;
        private static readonly int MinArgumentCount = 2;
        private static readonly int MaxArgumentCount = 3;
        private static readonly HashSet<string> KnownFormats;

        static Format()
        {
            KnownFormats = new HashSet<string>(StringComparer.Ordinal)
            {
                "c",
                "C", // currency
                "d",
                "D", // digits / day
                "e",
                "E", // exponential
                "f",
                "F", // fixed point / full datetime
                "g",
                "G", // general
                "m",
                "M", // month
                "n",
                "N",
                "o",
                "O", // datetime
                "r",
                "R", // RFC1123 datetime
                "s", // sortable datetime
                "t",
                "T", // time
                "u",
                "U", // universal datetime
                "x",
                "X", // hexadecimal
                "y",
                "Y", // year
            };
        }

        public Format()
        : base(FuncName, MinArgumentCount, MaxArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<FormatArgs> call)
        {
            // TODO : include into boolean expression after implementing
            // of any type value support
            ValidationScenario
                .For("VALUE", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                .Then(v => call.ValidatedArgs.SourceValue = v);

            return ValidationScenario
                    .For("FORMAT", call.RawArgs[1], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.FormatString = s)
                && (call.RawArgs.Count < 3
                || ValidationScenario
                    .For("CULTURE", call.RawArgs[2], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidStr.Validate)
                    .Then(c => call.ValidatedArgs.CultureCode = c));
        }

        protected override string DoEvaluateResultType(CallSignature<FormatArgs> call) => ResultType;

        // TODO : support more scenarios
        protected override SqlValue DoEvaluateResultValue(CallSignature<FormatArgs> call)
        {
            if (call.ValidatedArgs.SourceValue is SqlStrTypeValue)
            {
                call.Context.InvalidArgument("Only numeric and datetime types are supported");
                return default;
            }

            if (call.ValidatedArgs.SourceValue?.IsNull ?? false)
            {
                call.Context.RedundantCall("Value is NULL");
                return call.ResultTypeHandler.ValueFactory.NewNull(call.Context.Node);
            }

            if (call.ValidatedArgs.FormatString.IsNull)
            {
                call.Context.RedundantCall("FORMAT is NULL");
                return call.ResultTypeHandler.ValueFactory.NewNull(call.Context.Node);
            }

            if (call.ValidatedArgs.FormatString.EstimatedSize == 0)
            {
                call.Context.RedundantCall("FORMAT is empty");
                var srcAsStr = call.Context.Converter.ImplicitlyConvert<SqlStrTypeValue>(call.ValidatedArgs.SourceValue);

                // null if could not convert, precise or approximate value if it suceeded
                return srcAsStr;
            }

            if (call.ValidatedArgs.FormatString.EstimatedSize > 1)
            {
                // if there is a custom template then the output will always be of given template length
                return call.ValidatedArgs.FormatString.ChangeTo(call.ValidatedArgs.FormatString.EstimatedSize, call.Context.NewSource);
            }

            if (call.ValidatedArgs.SourceValue is SqlIntTypeValue srcInt
            && call.ValidatedArgs.FormatString.IsPreciseValue)
            {
                var format = call.ValidatedArgs.FormatString.Value;
                if (KnownFormats.Contains(format))
                {
                    string formattedValue = srcInt.EstimatedSize.High.ToString(format);
                    return call.ValidatedArgs.FormatString.ChangeTo(formattedValue.Length, call.Context.NewSource);
                }
                else
                {
                    call.Context.ArgumentOutOfRange("FORMAT", $"'{format}' is not supported");
                    return call.ResultTypeHandler.ValueFactory.NewNull(call.Context.Node);
                }
            }

            var str = call.Context.Converter.ImplicitlyConvert<SqlStrTypeValue>(call.ValidatedArgs.SourceValue);
            if (str != null)
            {
                // currency symbol and thousand separators cannot add more symbols then
                // the value already has
                return call.ValidatedArgs.FormatString.ChangeTo(str.EstimatedSize * 2, call.Context.NewSource);
            }

            return default;
        }

        public class FormatArgs
        {
            public SqlStrTypeValue FormatString { get; set; }

            public SqlValue SourceValue { get; set; }

            // TODO : support it for real
            public SqlStrTypeValue CultureCode { get; set; }
        }
    }
}
