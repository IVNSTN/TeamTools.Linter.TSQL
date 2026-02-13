using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // See also LiteralContainsLookAlikeCharRule, AlphabetMixInIdentifierRule
    [RuleIdentity("NM0854", "IDENTIFIER_LOOK_ALIKE_CHAR")]
    internal sealed class IdentifierContainsLookAlikeCharRule : AbstractRule
    {
        private const int MinIdentifierLength = 2;

        private readonly Action<Identifier, string> validator;

        public IdentifierContainsLookAlikeCharRule() : base()
        {
            validator = new Action<Identifier, string>(ValidateIdentifier);
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var ident = new DatabaseObjectIdentifierDetector(validator, true);
            node.AcceptChildren(ident);
        }

        private void ValidateIdentifier(Identifier node, string name)
        {
            if (string.IsNullOrEmpty(name)
            || name.Length < MinIdentifierLength)
            {
                return;
            }

            ValidateChars(name, node.StartLine, node.StartColumn);
        }

        private void ValidateChars(string text, int line, int col) => LookAlikeCharDetector.ValidateChars(text, line, col, ViolationHandlerPerLine);
    }
}
