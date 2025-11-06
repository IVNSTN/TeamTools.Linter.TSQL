using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// ScriptDom integration.
    /// </summary>
    public partial class AbstractRule : TSqlFragmentVisitor, ISqlRule
    {
        public void Validate(TSqlFragment node) => node.Accept(this);
    }
}
