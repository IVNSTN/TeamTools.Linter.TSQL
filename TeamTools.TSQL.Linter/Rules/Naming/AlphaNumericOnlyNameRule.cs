using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0461", "NON_ALPHANUMERIC_NAME")]
    internal sealed class AlphaNumericOnlyNameRule : AbstractRule
    {
        private static readonly Regex AlphanumericPattern = new Regex(
            @"^(@|#|##)?[a-zA-Z0-9 _-]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly Action<Identifier, string> validator;

        public AlphaNumericOnlyNameRule() : base()
        {
            validator = new Action<Identifier, string>(ValidateIdentifier);
        }

        protected override void ValidateScript(TSqlScript node)
            => node.AcceptChildren(new DatabaseObjectIdentifierDetector(validator, definitionOnly: true, ignoreVariables: false, ignoreAliases: true));

        private void ValidateIdentifier(Identifier identifier, string name)
        {
            if (identifier is null || string.IsNullOrEmpty(name))
            {
                return;
            }

            if (AlphanumericPattern.IsMatch(name))
            {
                return;
            }

            HandleNodeError(identifier, name);
        }
    }
}
