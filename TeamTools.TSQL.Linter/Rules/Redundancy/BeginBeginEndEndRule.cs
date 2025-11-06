using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0221", "BEGIN_BEGIN_END_END")]
    internal sealed class BeginBeginEndEndRule : AbstractRule
    {
        public BeginBeginEndEndRule() : base()
        {
        }

        public override void Visit(BeginEndBlockStatement node)
        {
            var stmtVisitor = new BlockVisitor();

            node.AcceptChildren(stmtVisitor);

            if (stmtVisitor.NestedBeginEnds == 0)
            {
                return;
            }

            if (stmtVisitor.ThisBlockStatements > 0)
            {
                return;
            }

            HandleNodeError(node);
        }

        private class BlockVisitor : TSqlFragmentVisitor
        {
            public int NestedBeginEnds { get; private set; } = 0;

            public int ThisBlockStatements { get; private set; } = 0;

            public override void Visit(TSqlStatement node)
            {
                if (node is BeginEndBlockStatement)
                {
                    NestedBeginEnds++;

                    var v = new BlockVisitor();
                    node.AcceptChildren(v);

                    ThisBlockStatements -= v.ThisBlockStatements;
                }
                else
                {
                    ThisBlockStatements++;
                }
            }
        }
    }
}
