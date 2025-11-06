using System;
using System.Collections.Generic;
using System.Text;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class QuoteName : SqlGenericFunctionHandler<QuoteName.QuoteNameArgs>
    {
        private static readonly string FuncName = "QUOTENAME";
        private static readonly string ResultType = "dbo.NVARCHAR";
        // https://learn.microsoft.com/en-us/sql/t-sql/functions/quotename-transact-sql
        private static readonly int InputSizeLimit = 128; // dbo.SYSNAME
        private static readonly int ResultSizeLimit = 258;
        private static readonly string DefaultQuoteChar = "[";

        private static readonly IDictionary<string, OpenCloseChar> AllowedQuoteChars = new SortedDictionary<string, OpenCloseChar>(StringComparer.OrdinalIgnoreCase);

        static QuoteName()
        {
            RegisterQuotes('[', ']');
            RegisterQuotes('{', '}');
            RegisterQuotes('(', ')');
            RegisterQuotes('<', '>');
            RegisterQuotes('`', '`');
            RegisterQuotes('\'', '\'');
            RegisterQuotes('"', '"');
        }

        public QuoteName() : base(FuncName, 1, 2)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<QuoteNameArgs> call)
        {
            return (ValidationScenario
                    .For("NAME", call.RawArgs[0], call.Context)
                    .When<SqlValue>(ArgumentIsValue.Validate)
                    .And<SqlStrTypeValue>(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.Name = s)
                && (call.RawArgs.Count < 2
                || ValidationScenario
                    .For("QUOTED_CHAR", call.RawArgs[1], call.Context)
                    .When<SqlValue>(ArgumentIsValue.Validate)
                    .And<SqlStrTypeValue>(ArgumentIsValidStr.Validate)
                    .Then(s => call.ValidatedArgs.QuoteChar = s)))
                // if we could not parse source name we still can
                // estimate to approximate string size
                || true;
        }

        protected override string DoEvaluateResultType(CallSignature<QuoteNameArgs> call) => ResultType;

        protected override SqlValue DoEvaluateResultValue(CallSignature<QuoteNameArgs> call)
        {
            if (call.ValidatedArgs.Name is null)
            {
                return call.Context.Converter
                    .ImplicitlyConvert<SqlStrTypeValue>(base.DoEvaluateResultValue(call))?
                    .ChangeTo(ResultSizeLimit, call.Context.NewSource);
            }

            if (call.ValidatedArgs.Name.EstimatedSize > InputSizeLimit && !call.ValidatedArgs.Name.IsNull)
            {
                call.Context.ArgumentOutOfRange("SRC", "string is too long");
                return call.ValidatedArgs.Name.TypeHandler.StrValueFactory.MakeNull(call.Context.Node);
            }

            if (call.ValidatedArgs.Name.IsNull || (call.ValidatedArgs.QuoteChar?.IsNull ?? false))
            {
                call.Context.RedundantCall("Source name or quotation char is NULL");
                return call.ValidatedArgs.Name.TypeHandler.StrValueFactory.MakeNull(call.Context.Node);
            }

            if (call.ValidatedArgs.Name.EstimatedSize == 0)
            {
                call.Context.RedundantCall("Source name is empty");
            }

            if (call.ValidatedArgs.QuoteChar != null)
            {
                // Empty string is not "supported" but is a valid input
                // default "[" will be used, result will be not NULL
                if (call.ValidatedArgs.QuoteChar.IsPreciseValue
                && !string.IsNullOrEmpty(call.ValidatedArgs.QuoteChar.Value)
                && !IsSupportedQuotationSymbol(call.ValidatedArgs.QuoteChar.Value))
                {
                    call.Context.RedundantCall("Given quotation char is not supported");
                    return call.ValidatedArgs.Name.TypeHandler.StrValueFactory.MakeNull(call.Context.Node);
                }

                if (call.ValidatedArgs.QuoteChar.EstimatedSize == 0)
                {
                    // TODO : not redundant call but redundant argument
                    call.Context.RedundantCall("No quotation char provided. Default will be used");
                }
                else if (call.ValidatedArgs.QuoteChar.EstimatedSize > 1)
                {
                    call.Context.ImplicitTruncation(1, call.ValidatedArgs.QuoteChar.EstimatedSize, "Quoted char accepts only first char");
                }
            }

            if (call.ValidatedArgs.Name.IsPreciseValue && (call.ValidatedArgs.QuoteChar?.IsPreciseValue ?? true))
            {
                string result = BuildQuotedName(call.ValidatedArgs.Name, call.ValidatedArgs.QuoteChar);

                if (result.Length > ResultSizeLimit)
                {
                    call.Context.ImplicitTruncation(ResultSizeLimit, result.Length, result);
                    result = result.Substring(0, ResultSizeLimit);
                }

                return call.ValidatedArgs.Name.ChangeTo(result, call.Context.NewSource);
            }

            // Considering there is nothing to replace
            // just open + close quotation chars will be added
            int resultSize = call.ValidatedArgs.Name.EstimatedSize + 2;
            if (resultSize > ResultSizeLimit)
            {
                resultSize = ResultSizeLimit;
            }

            return call.ValidatedArgs.Name.ChangeTo(resultSize, call.Context.NewSource);
        }

        private static bool IsSupportedQuotationSymbol(string bracket)
        {
            return !string.IsNullOrEmpty(bracket)
                && AllowedQuoteChars.ContainsKey(bracket.Substring(0, 1));
        }

        private static string BuildQuotedName(SqlStrTypeValue sourceString, SqlStrTypeValue quotationSymbol)
        {
            string quoteChar = quotationSymbol?.Value;
            quoteChar = string.IsNullOrEmpty(quoteChar) ? DefaultQuoteChar : quoteChar.Substring(0, 1);

            var openClose = AllowedQuoteChars[quoteChar];

            return new StringBuilder()
                .Append(openClose.Open)
                .Append(sourceString.Value.Replace(openClose.Close, openClose.Replacement))
                .Append(openClose.Close)
                .ToString();
        }

        private static void RegisterQuotes(char open, char close)
            => AllowedQuoteChars.Add(open.ToString(), new OpenCloseChar(open, close));

        public class QuoteNameArgs
        {
            public SqlStrTypeValue Name { get; set; }

            public SqlStrTypeValue QuoteChar { get; set; }
        }

        private class OpenCloseChar
        {
            public OpenCloseChar(char open, char close)
            {
                Open = open.ToString();
                Close = close.ToString();

                Replacement = new string(close, 2);
            }

            public string Open { get; }

            public string Close { get; }

            public string Replacement { get; }
        }
    }
}
