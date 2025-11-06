using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0172", "DISPOSABLE_LOOP")]
    internal sealed class UnrepeatableLoopRule : AbstractRule
    {
        public UnrepeatableLoopRule() : base()
        {
        }

        public override void Visit(WhileStatement node)
        {
            IDictionary<int, bool> checkedNodes = new Dictionary<int, bool>();
            var blockVisitor = new RecursiveQuitBlockVisitor(
                checkedNodes,
                (i, quitType) =>
                {
                    if (quitType == TSqlTokenType.Continue)
                    {
                        // continue is fine - entering next loop
                        return;
                    }

                    HandleNodeError(i);
                },
                new QuitBlockParserState());
            // AcceptChildren - to handle all the exits as non-conditional
            node.AcceptChildren(blockVisitor);
        }
    }
}
