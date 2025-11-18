using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0807", "GLOBAL_CURSOR_REF")]
    [CursorRule]
    internal sealed class GlobalCursorReferenceRule : AbstractRule
    {
        public GlobalCursorReferenceRule() : base()
        {
        }

        public override void Visit(UpdateSpecification node) => ValidateCursor(node.WhereClause?.Cursor);

        public override void Visit(DeleteSpecification node) => ValidateCursor(node.WhereClause?.Cursor);

        private void ValidateCursor(CursorId cursor)
        {
            if (cursor != null && cursor.IsGlobal)
            {
                HandleNodeError(cursor);
            }
        }
    }
}
