using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    internal class HashBytes : SqlGenericFunctionHandler<HashBytes.HashBytesArgs>
    {
        private static readonly string FuncName = "HASHBYTES";
        private static readonly int RequiredArgumentCount = 2;
        private static readonly string OutputType = TSqlDomainAttributes.Types.VarBinary;

        public HashBytes() : base(FuncName, RequiredArgumentCount)
        {
        }

        // TODO : verify that input is either string or binary data
        public override bool ValidateArgumentValues(CallSignature<HashBytesArgs> call)
        {
            return ValidationScenario
                .For("INPUT_VALUE", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                .Then(s => call.ValidatedArgs.Input = s);
        }

        protected override string DoEvaluateResultType(CallSignature<HashBytesArgs> call) => OutputType;

        [ExcludeFromCodeCoverage]
        public sealed class HashBytesArgs
        {
            public SqlValue Input { get; set; }
        }
    }
}
