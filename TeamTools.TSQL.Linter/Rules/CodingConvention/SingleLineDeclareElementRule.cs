using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0226", "MULTILINE_DECLARE_ELEMENT")]
    internal sealed class SingleLineDeclareElementRule : AbstractRule
    {
        public SingleLineDeclareElementRule() : base()
        {
        }

        public override void Visit(DeclareVariableElement node)
        {
            // some parser bug
            if (node.FirstTokenIndex < 0)
            {
                return;
            }

            int firstLine = node.ScriptTokenStream[node.FirstTokenIndex].Line;
            int lastLine = node.ScriptTokenStream[node.LastTokenIndex].Line;

            // multiline strings are treated as a single node
            if ((null != node.Value) && (node.Value is StringLiteral str))
            {
                lastLine += str.Value.LineCount() - 1;
            }

            if (firstLine == lastLine)
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
