using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0265", "CAPS_IDENTIFIER")]
    internal class CapsIdentifierRule : AbstractRule
    {
        private const int MinSymbolCount = 3;
        private static readonly Regex NonWordChars = new Regex("[^a-zA-Zа-яА-Я]+", RegexOptions.Compiled);

        private readonly Action<Identifier, string> validator;

        public CapsIdentifierRule() : base()
        {
            validator = new Action<Identifier, string>(ValidateIdentifier);
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var id = new DatabaseObjectIdentifierDetector(validator, true);
            node.AcceptChildren(id);
        }

        protected void ValidateIdentifier(Identifier node, string name)
        {
            if (node is null || string.IsNullOrEmpty(name))
            {
                return;
            }

            if (name.Length < MinSymbolCount)
            {
                // to avoid sanitizing below if it is already shorter
                return;
            }

            string sanitizedName = SanitizeName(name);

            if (sanitizedName.Length < MinSymbolCount)
            {
                return;
            }

            if (!name.IsUpperCase())
            {
                return;
            }

            HandleNodeError(node, name);
        }

        private static string SanitizeName(string name)
        {
            return NonWordChars.Replace(name, "").Trim();
        }
    }
}
