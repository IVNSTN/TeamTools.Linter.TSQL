using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
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
        private static readonly Regex PhoneRegex = new Regex("(?<output>(?<country>([+]||[0]{2,})\\d{1,2})[(\\s-]*(?<prefix>\\d{3})[)\\s-]*(?<number>(((?:\\d)[\\s-]*){7,9})(!=[\\d])?))", RegexOptions.Compiled);
        private static readonly Regex AnsiDateRegex = new Regex("^(?<date>(?<year>\\d{4})[-.\\s]{1}(?<month>\\d{1,2})[-.\\s]{1}(?<day>\\d{1,2})(!=[\\d])?)", RegexOptions.Compiled);
        private static readonly Regex GermanDateRegex = new Regex("^(?<date>(?<month>\\d{1,2})[-.\\s]{1}(?<day>\\d{1,2})[-.\\s]{1}(?<year>\\d{4})(!=[\\d])?)", RegexOptions.Compiled);
        private static readonly Regex GuidRegex = new Regex("[\\da-fA-F]{8}-([\\da-fA-F]{4}-){3}[\\da-fA-F]{12}", RegexOptions.Compiled);
        private static readonly Regex GuidStringRegex = new Regex("^(['\"\\s(){}]*)?[\\da-fA-F]{8}-([\\da-fA-F]{4}-){3}[\\da-fA-F]{12}(['\"\\s(){}]*)?$", RegexOptions.Compiled);
        private static readonly Regex RemoveNonDigits = new Regex("[^\\d]", RegexOptions.Compiled);

        private readonly Action<TSqlParserToken, string> validator;

        public PersonalDataPhoneRule() : base()
        {
            validator = new Action<TSqlParserToken, string>(MakeFinalValidation);
        }

        protected override void ValidateScript(TSqlScript node)
        {
            StringRegexMatchVisitor.DetectMatch(node, PhoneRegex, validator, 7);
        }

        private static bool LooksLikePhone(string value, string fullString)
        {
            const string GeneralPhonePrefix = "+";
            const int MaxPhoneLength = 11;

            // if no starting with + then these numbers can be something else
            if (value.StartsWith(GeneralPhonePrefix))
            {
                return true;
            }

            string cleanedValue = RemoveNonDigits.Replace(value, "");

            // too long value might be something else
            if (cleanedValue.Length > MaxPhoneLength)
            {
                return false;
            }

            // if only a few digits used then this is more likely a magic number e.g. 99999999, not a phone
            if (cleanedValue.Distinct().Count() < 3)
            {
                return false;
            }

            // dates and guids a like phones
            if (AnsiDateRegex.IsMatch(value))
            {
                return false;
            }

            if (GermanDateRegex.IsMatch(value))
            {
                return false;
            }

            if (GuidRegex.IsMatch(value) || GuidStringRegex.IsMatch(fullString))
            {
                return false;
            }

            return true;
        }

        private void MakeFinalValidation(TSqlParserToken token, string value)
        {
            if (!LooksLikePhone(value, token.Text))
            {
                return;
            }

            // TODO : need better violation positioning
            HandleTokenError(token);
        }
    }
}
