using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Interfaces
{
    public interface ISqlRule : ILinterRule
    {
        void Validate(TSqlFragment node);
    }
}
