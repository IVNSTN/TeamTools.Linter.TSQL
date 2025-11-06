using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// PROCEDURE/PROC spelling.
    /// </summary>
    internal partial class KeywordFullOrShortVersionRule
    {
        public override void Visit(ProcedureStatementBodyBase node)
        {
            int lastToken;

            if ((node.Parameters?.Count ?? 0) > 0)
            {
                lastToken = node.Parameters[0].FirstTokenIndex;
            }
            else if ((node.StatementList?.Statements?.Count ?? 0) > 0)
            {
                lastToken = node.StatementList.Statements[0].FirstTokenIndex;
            }
            else
            {
                lastToken = node.LastTokenIndex;
            }

            ValidateSpelling(
                KeywordWithShorthand.Proc,
                node.FirstTokenIndex,
                lastToken,
                node);
        }

        public override void Visit(DropProcedureStatement node)
        {
            ValidateSpelling(
                KeywordWithShorthand.Proc,
                node.FirstTokenIndex,
                node.Objects[0].FirstTokenIndex,
                node);
        }
    }
}
