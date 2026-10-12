using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0466", "TRAILING_SPACE_IN_NAME")]
    internal sealed class TrailingSpaceInNameRule : AbstractRule
    {
        private readonly Action<Identifier, string> nameValidator;

        public TrailingSpaceInNameRule() : base()
        {
            nameValidator = new Action<Identifier, string>(ValidateIdentifier);
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var ident = new DatabaseObjectIdentifierDetector(nameValidator, definitionOnly: true);
            node.AcceptChildren(ident);
        }

        private void ValidateIdentifier(Identifier node, string name)
        {
            if (name[name.Length - 1] == ' ')
            {
                HandleNodeError(node);
            }
        }
    }
}
