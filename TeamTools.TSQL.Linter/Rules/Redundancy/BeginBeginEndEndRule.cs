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
            bool blockHasSomethingElse = false;
            var statements = node.StatementList.Statements;
            int n = statements.Count;
            for (int i = 0; i < n; i++)
            {
                // TODO : this algorithm does not really look like a system
                if (statements[i] is BeginEndBlockStatement be)
                {
                    if (!blockHasSomethingElse)
                    {
                        HandleTokenError(node.ScriptTokenStream[be.FirstTokenIndex]);
                    }
                }
                else
                {
                    blockHasSomethingElse = true;
                }
            }
        }
    }
}
