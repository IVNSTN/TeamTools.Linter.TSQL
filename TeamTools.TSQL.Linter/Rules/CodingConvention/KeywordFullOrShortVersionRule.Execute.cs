using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// EXECUTE/EXEC spelling.
    /// </summary>
    internal partial class KeywordFullOrShortVersionRule
    {
        public override void Visit(ExecuteStatement node)
        {
            ValidateSpelling(
                KeywordWithShorthand.Exec,
                node.FirstTokenIndex,
                node.FirstTokenIndex, // execute is the first word
                node);
        }
    }
}
