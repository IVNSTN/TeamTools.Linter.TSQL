using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// OUT/OUTPUT spelling.
    /// </summary>
    internal partial class KeywordFullOrShortVersionRule
    {
        public override void Visit(ProcedureParameter node)
        {
            if (!node.IsOutput())
            {
                return;
            }

            ValidateSpelling(
                KeywordWithShorthand.Output,
                node.DataType?.LastTokenIndex + 1 ?? node.FirstTokenIndex,
                node.LastTokenIndex,
                node);
        }

        public override void Visit(ExecuteParameter node)
        {
            if (!node.IsOutput)
            {
                return;
            }

            ValidateSpelling(
                KeywordWithShorthand.Output,
                node.ParameterValue?.FirstTokenIndex + 1 ?? node.FirstTokenIndex,
                node.LastTokenIndex,
                node);
        }
    }
}
