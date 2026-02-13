using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0841", "INVISIBLE_CHAR_IN_IDENTIFIER")]
    internal class IdentifierHasInvisibleCharRule : AbstractRule
    {
        public IdentifierHasInvisibleCharRule() : base()
        {
        }

        public override void Visit(Identifier node)
        {
            int badCharPos = InvisibleCharDetector.LocateInvisibleChar(node.Value, out string symbolName);
            if (badCharPos >= 0)
            {
                HandleLineError(node.StartLine, node.StartColumn + badCharPos, symbolName);
            }
        }
    }
}
