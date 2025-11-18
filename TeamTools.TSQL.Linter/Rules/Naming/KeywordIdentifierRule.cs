using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0263", "KEYWORD_IDENTIFIER")]
    internal sealed class KeywordIdentifierRule : AbstractRule, IKeywordDetector
    {
        private HashSet<string> reservedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private Action<Identifier, string> validator;

        public KeywordIdentifierRule() : base()
        {
        }

        public void LoadKeywords(ICollection<string> values)
        {
            reservedWords = new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
        }

        protected override void ValidateBatch(TSqlBatch node)
           => node.AcceptChildren(new DatabaseObjectIdentifierDetector(validator ?? (validator = new Action<Identifier, string>(ValidateIdentifier)), true, true, false));

        private void ValidateIdentifier(Identifier node, string name)
        {
            if (node is null || string.IsNullOrEmpty(name))
            {
                return;
            }

            if (reservedWords.Contains(name))
            {
                HandleNodeError(node, name);
            }
        }
    }
}
