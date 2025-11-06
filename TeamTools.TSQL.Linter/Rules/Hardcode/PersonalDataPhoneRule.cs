using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("HD0258", "PERSONAL_DATA_PHONE")]
    internal sealed class PersonalDataPhoneRule : AbstractRule
    {
        // TODO : very similar to email detector, only regex is different
        private readonly Regex phoneRegex = new Regex("(?<output>(?<country>([+]||[0]{2,})\\d{1,2})[(\\s-]*(?<prefix>\\d{3})[)\\s-]*(?<number>(((?:\\d)[\\s-]*){7,9})(!=[\\d])?))");
        private readonly Regex ansiDateRegex = new Regex("^(?<date>(?<year>\\d{4})[-.\\s]{1}(?<month>\\d{1,2})[-.\\s]{1}(?<day>\\d{1,2})(!=[\\d])?)");
        private readonly Regex germanDateRegex = new Regex("^(?<date>(?<month>\\d{1,2})[-.\\s]{1}(?<day>\\d{1,2})[-.\\s]{1}(?<year>\\d{4})(!=[\\d])?)");
        private readonly Regex guidRegex = new Regex("[\\da-fA-F]{8}-([\\da-fA-F]{4}-){3}[\\da-fA-F]{12}");
        private readonly Regex guidStringRegex = new Regex("^(['\"\\s(){}]*)?[\\da-fA-F]{8}-([\\da-fA-F]{4}-){3}[\\da-fA-F]{12}(['\"\\s(){}]*)?$");

        public PersonalDataPhoneRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var regexMatchVisitor = new StringRegexMatchVisitor(
                phoneRegex,
                true,
                (token, value, fullString) =>
                {
                    // if no starting with + then these numbers can be something else
                    if (!value.StartsWith("+"))
                    {
                        string cleanedValue = value;
                        cleanedValue = Regex.Replace(cleanedValue, "[^\\d]", "");
                        // if only a few digits used then this is more likely a magic number e.g. 99999999, not a phone
                        if (cleanedValue.Distinct().ToArray().Length < 3)
                        {
                            return;
                        }

                        // too long value might be something else
                        if (cleanedValue.Length > 11)
                        {
                            return;
                        }

                        // dates and guids a like phones
                        if (ansiDateRegex.IsMatch(value))
                        {
                            return;
                        }

                        if (germanDateRegex.IsMatch(value))
                        {
                            return;
                        }

                        if (guidRegex.IsMatch(value) || guidStringRegex.IsMatch(fullString))
                        {
                            return;
                        }
                    }

                    HandleTokenError(node.ScriptTokenStream[token]);
                });
            regexMatchVisitor.DetectMatch(node);
        }
    }
}
