using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0292", "CURSOR_NOT_INLINE")]
    internal sealed class CursorDeclareInlineRule : AbstractRule
    {
        public CursorDeclareInlineRule() : base()
        {
        }

        public override void Visit(DeclareVariableElement node)
        {
            if (node.DataType is SqlDataTypeReference sd && sd.SqlDataTypeOption == SqlDataTypeOption.Cursor)
            {
                HandleNodeError(node);
            }
        }
    }
}
