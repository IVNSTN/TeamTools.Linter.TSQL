using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0137", "IF_BEGIN_END")]
    internal sealed class BeginEndForIfRule : AbstractRule
    {
        public BeginEndForIfRule() : base()
        {
        }

        public override void Visit(IfStatement node)
            => ValidateBlock(node.ThenStatement).ValidateBlock(node.ElseStatement, true);

        private static bool IsBlockValid(TSqlStatement node, bool respectNestedIf)
            => node is BeginEndBlockStatement || (respectNestedIf && node is IfStatement);

        private BeginEndForIfRule ValidateBlock(TSqlStatement node, bool respectNestedIf = false)
        {
            if (node != null && !IsBlockValid(node, respectNestedIf))
            {
                HandleNodeError(node);
            }

            return this;
        }
    }
}
