using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class CursorStatus : SqlGenericFunctionHandler<CursorStatus.CursorStatusArgs>
    {
        private static readonly string FuncName = "CURSOR_STATUS";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.SmallInt;
        private static readonly int RequiredArgCount = 2;
        private static readonly SqlIntValueRange StatusValueRange = new SqlIntValueRange(-3, 1);

        private static readonly HashSet<string> ValidScopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "local",
            "global",
            "variable",
        };

        public CursorStatus() : base(FuncName, RequiredArgCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<CursorStatusArgs> call)
        {
            return ValidationScenario
                    .For("CURSOR_SCOPE", call.RawArgs[0], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.CursorScope = s)
                && ValidationScenario
                    .For("CURSOR_NAME", call.RawArgs[1], call.Context)
                    .When(ArgumentIsValue.Validate)
                    .And(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.CursorName = s);
        }

        protected override string DoEvaluateResultType(CallSignature<CursorStatusArgs> call) => ResultTypeName;

        protected override SqlValue DoEvaluateResultValue(CallSignature<CursorStatusArgs> call)
        {
            var res = call.Context.Converter.ImplicitlyConvert<SqlIntTypeValue>(base.DoEvaluateResultValue(call))
                .ChangeTo(StatusValueRange, call.Context.NewSource);

            if (call.ValidatedArgs.CursorScope.IsNull || call.ValidatedArgs.CursorName.IsNull)
            {
                // TODO : translate
                call.Context.InvalidArgument(FuncName + " requires both args");
                return res;
            }

            if (call.ValidatedArgs.CursorScope.IsPreciseValue)
            {
                var cursorScope = call.ValidatedArgs.CursorScope.Value;

                if (!ValidScopes.Contains(cursorScope))
                {
                    call.Context.InvalidArgument(cursorScope);
                }
            }

            // TODO : Check variable existence when cursorScope == 'variable'
            // after implementing CURSOR variables support (registering variables of any type) in the evaluator
            if (call.ValidatedArgs.CursorName.IsPreciseValue && string.IsNullOrWhiteSpace(call.ValidatedArgs.CursorName.Value))
            {
                // TODO : translate
                call.Context.InvalidArgument("cursor name must not be empty");
            }

            return res;
        }

        [ExcludeFromCodeCoverage]
        public sealed class CursorStatusArgs
        {
            public SqlStrTypeValue CursorScope { get; set; }

            public SqlStrTypeValue CursorName { get; set; }
        }
    }
}
