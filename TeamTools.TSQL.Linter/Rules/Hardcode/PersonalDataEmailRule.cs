using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("HD0257", "PERSONAL_DATA_EMAIL")]
    internal sealed class PersonalDataEmailRule : AbstractRule
    {
        // \w before @ is a simplification to avoid XML patterns false positive matches
        private static readonly Regex EmailRegex = new Regex(
            @"(?<output>[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})",
            RegexOptions.Compiled);

        private static readonly Regex CommentedVariable = new Regex(@"^\W+@", RegexOptions.Compiled);

        private readonly Action<TSqlParserToken> validator;

        public PersonalDataEmailRule() : base()
        {
            validator = new Action<TSqlParserToken>(MakeFinalValidation);
        }

        protected override void ValidateScript(TSqlScript node)
        {
            StringRegexMatchVisitor.DetectMatch(node, '@', validator, 3);
        }

        private void MakeFinalValidation(TSqlParserToken token)
        {
            var emailCandidate = EmailRegex.Match(token.Text)?.Groups["output"].Value;
            if (string.IsNullOrEmpty(emailCandidate))
            {
                return;
            }

            // very likely a commented variable e.g. --@var which is a valid email but not what we want
            if (CommentedVariable.IsMatch(emailCandidate))
            {
                return;
            }

            // TODO : need better violation positioning
            HandleTokenError(token);
        }
    }
}
