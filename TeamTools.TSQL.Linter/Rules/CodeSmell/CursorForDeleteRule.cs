using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0804", "CURSOR_FOR_DELETE")]
    [CursorRule]
    internal sealed class CursorForDeleteRule : AbstractRule
    {
        public CursorForDeleteRule() : base()
        {
        }

        // TODO : Detect logically similar behaviour but not via WHERE CURRENT OF
        public override void Visit(DeleteSpecification node) => HandleNodeErrorIfAny(node.WhereClause?.Cursor);
    }
}
