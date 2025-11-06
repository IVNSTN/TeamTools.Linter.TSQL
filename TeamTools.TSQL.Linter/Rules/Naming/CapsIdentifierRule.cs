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
        private readonly Regex nonWordChars = new Regex("[^a-zA-Zа-яА-Я]+", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public CapsIdentifierRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var id = new DatabaseObjectIdentifierDetector(ValidateIdentifier, true);
            node.AcceptChildren(id);
        }

        protected void ValidateIdentifier(Identifier node, string name)
        {
            if (node is null || string.IsNullOrEmpty(name))
            {
                return;
            }

            string sanitizedName = nonWordChars.Replace(name, "").Trim();

            if (sanitizedName.Length < MinSymbolCount)
            {
                return;
            }

            if (!name.ToUpperInvariant().Equals(name, StringComparison.InvariantCulture))
            {
                return;
            }

            HandleNodeError(node, name);
        }
    }
}
