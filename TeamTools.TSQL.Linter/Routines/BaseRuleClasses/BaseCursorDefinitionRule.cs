using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.Linter.Routines
{
    internal abstract class BaseCursorDefinitionRule : AbstractRule
    {
        protected BaseCursorDefinitionRule() : base()
        {
        }

        public override void Visit(SetVariableStatement node)
        {
            if (node.CursorDefinition != null)
            {
                ValidateCursor(node.Variable.Name, node.CursorDefinition);
            }
        }

        public override void Visit(DeclareCursorStatement node)
        {
            ValidateCursor(node.Name.Value, node.CursorDefinition);
        }

        protected abstract void ValidateCursor(string cursorName, CursorDefinition node);
    }
}
