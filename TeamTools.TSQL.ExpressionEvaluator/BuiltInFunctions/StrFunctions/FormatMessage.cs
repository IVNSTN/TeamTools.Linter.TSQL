using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    // https://learn.microsoft.com/en-us/sql/t-sql/functions/formatmessage-transact-sql
    public class FormatMessage : SqlGenericFunctionHandler<FormatMessage.FormatArgs>
    {
        private static readonly string FuncName = "FORMATMESSAGE";
        private static readonly int MinArgumentCount = 1;
        private static readonly int MaxArgumentCount = 21; // template + 20 args
        private static readonly int MaxSupportedLength = 2047;

        public FormatMessage()
        : base(FuncName, MinArgumentCount, MaxArgumentCount)
        {
        }

        public override bool ValidateArgumentValues(CallSignature<FormatArgs> call)
        {
            return ValidationScenario
                .For("TEMPLATE", call.RawArgs[0], call.Context)
                .When(ArgumentIsValue.Validate)
                .And(ArgumentIsValidStr.Validate)
                .Then(s => call.ValidatedArgs.TemplateString = s)
                && AllArgsAreValues.Validate(call.RawArgs.Skip(1).ToList(), call.Context, v => call.ValidatedArgs.Args.Add(v));
        }

        protected override string DoEvaluateResultType(CallSignature<FormatArgs> call)
            => call.ValidatedArgs.TemplateString.TypeName;

        protected override SqlValue DoEvaluateResultValue(CallSignature<FormatArgs> call)
        {
            if (call.ValidatedArgs.TemplateString.IsNull || call.ValidatedArgs.TemplateString.EstimatedSize == 0)
            {
                call.Context.RedundantCall("Template is NULL or empty");
                return call.ValidatedArgs.TemplateString;
            }

            if (call.ValidatedArgs.Args.Count == 0)
            {
                call.Context.RedundantCall("No args to process");
                return call.ValidatedArgs.TemplateString;
            }

            if (!call.ValidatedArgs.TemplateString.IsPreciseValue)
            {
                // even if there is an estimation of template length,
                // arguments can make it longer
                return default;
            }

            var groups = GetWildcards(call.ValidatedArgs.TemplateString.Value);
            var wildcards = groups.Where(g => !string.IsNullOrEmpty(g.Type)).ToList();
            int argCount = call.ValidatedArgs.Args.Count;

            if (wildcards.Count != argCount)
            {
                call.Context.InvalidNumberOfArgs(wildcards.Count, argCount);
            }

            if (wildcards.Count == 0)
            {
                call.Context.RedundantCall("Template has no wildcards");
                return call.ValidatedArgs.TemplateString;
            }

            var res = BuildFormattedString(groups.ToList(), wildcards, call.ValidatedArgs.Args, call.Context);

            if (res.IsTooLong || !res.IsPrecise)
            {
                int len = res.IsTooLong || res.Result.Length > MaxSupportedLength ? MaxSupportedLength : res.Result.Length;

                return call.ValidatedArgs.TemplateString.ChangeTo(len, call.Context.NewSource);
            }

            return call.ValidatedArgs.TemplateString.ChangeTo(res.Result.ToString(), call.Context.NewSource);
        }

        private static IEnumerable<WildcardElement> GetWildcards(string template)
        {
            var matches = FormatMessageWildcardExtractor.ExtractElements(template);
            int n = matches.Count;
            for (int i = 0; i < n; i++)
            {
                var m = matches[i];

                yield return new WildcardElement
                {
                    TextBefore = m.Groups["before"].Value,
                    Prefix = m.Groups["prefix"].Value,
                    Size = m.Groups["size"].Value,
                    Type = m.Groups["type"].Value,
                    TextAfter = m.Groups["after"].Value,
                };
            }
        }

        private static BuildResult BuildFormattedString(List<WildcardElement> groups, List<WildcardElement> wildcards, List<SqlValue> args, EvaluationContext context)
        {
            var res = new StringBuilder();
            int argCount = args.Count;
            bool isPrecise = true;
            bool varcharMax = wildcards.Any(w => w.TextBefore.Length >= MaxSupportedLength || w.TextAfter.Length >= MaxSupportedLength)
            || args.Any(a => a is SqlStrTypeValue str && str.EstimatedSize >= MaxSupportedLength);

            int n = wildcards.Count;
            for (int i = 0; i < n; i++)
            {
                var wc = wildcards[i];

                res.Append(wc.TextBefore);

                if (i < argCount)
                {
                    // TODO : support different argument formatting options
                    var arg = context.Converter.ImplicitlyConvert<SqlStrTypeValue>(args[i]);

                    if (arg is null)
                    {
                        return default;
                    }

                    if (arg.EstimatedSize >= MaxSupportedLength)
                    {
                        varcharMax = true;
                        break;
                    }

                    if (arg.IsPreciseValue)
                    {
                        res.Append(arg.Value);
                    }
                    else
                    {
                        isPrecise = false;
                        res.Append(new string('?', arg.EstimatedSize));
                    }
                }

                res.Append(wc.TextAfter);

                if (res.Length >= MaxSupportedLength || varcharMax)
                {
                    break;
                }
            }

            if (!varcharMax)
            {
                var trailingText = groups[groups.Count - 1];
                if (string.IsNullOrEmpty(trailingText.Type)
                && !string.IsNullOrEmpty(trailingText.TextAfter))
                {
                    res.Append(trailingText.TextAfter);
                }
            }

            return new BuildResult
            {
                Result = res,
                IsPrecise = isPrecise,
                IsTooLong = varcharMax,
            };
        }

        private struct BuildResult
        {
            public StringBuilder Result;

            public bool IsPrecise;

            public bool IsTooLong;
        }

        public class FormatArgs
        {
            public SqlStrTypeValue TemplateString { get; set; }

            public List<SqlValue> Args { get; } = new List<SqlValue>();
        }

        [ExcludeFromCodeCoverage]
        public sealed class WildcardElement
        {
            public string TextBefore { get; set; }

            public string Prefix { get; set; }

            public string Size { get; set; }

            public string Type { get; set; }

            public string TextAfter { get; set; }
        }
    }
}
