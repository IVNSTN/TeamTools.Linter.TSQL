using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// TRAN/TRANSACTION spelling.
    /// </summary>
    internal partial class KeywordFullOrShortVersionRule
    {
        public override void Visit(TransactionStatement node)
        {
            ValidateSpelling(
                KeywordWithShorthand.Tran,
                node.FirstTokenIndex,
                node.Name?.FirstTokenIndex ?? node.LastTokenIndex,
                node);
        }

        public override void Visit(SetTransactionIsolationLevelStatement node)
        {
            ValidateSpelling(
                KeywordWithShorthand.Tran,
                node.FirstTokenIndex,
                node.LastTokenIndex,
                node);
        }
    }
}
