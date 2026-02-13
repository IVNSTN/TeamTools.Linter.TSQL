using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0793", "INVISIBLE_CHAR")]
    internal sealed class LiteralHasInvisibleCharRule : AbstractRule
    {
        public LiteralHasInvisibleCharRule() : base()
        {
        }

        public override void Visit(StringLiteral node)
        {
            int badCharPos = InvisibleCharDetector.LocateInvisibleChar(node.Value, out string symbolName);
            if (badCharPos >= 0)
            {
                HandleNodeError(node, string.Format(Strings.ViolationDetails_LiteralHasInvisibleCharRule_SymbolAtPos, symbolName, badCharPos.ToString()));
            }
        }
    }
}
