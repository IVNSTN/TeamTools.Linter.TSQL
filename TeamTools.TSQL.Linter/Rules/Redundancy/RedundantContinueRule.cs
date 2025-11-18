using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0730", "REDUNDANT_CONTINUE")]
    internal sealed class RedundantContinueRule : AbstractRule
    {
        public RedundantContinueRule() : base()
        {
        }

        public override void Visit(WhileStatement node)
        {
            var lastStatement = ExtractLastStatement(node.Statement);

            if (lastStatement != null && lastStatement is ContinueStatement)
            {
                HandleNodeError(lastStatement);
            }
        }

        private static TSqlStatement ExtractLastStatement(TSqlStatement stmt)
        {
            while (stmt is BeginEndBlockStatement be)
            {
                stmt = be.StatementList.Statements[be.StatementList.Statements.Count - 1];
            }

            if (stmt is IfStatement ifStmt)
            {
                return ExtractLastStatement(ifStmt.ElseStatement ?? ifStmt.ThenStatement);
            }

            return stmt;
        }
    }
}
