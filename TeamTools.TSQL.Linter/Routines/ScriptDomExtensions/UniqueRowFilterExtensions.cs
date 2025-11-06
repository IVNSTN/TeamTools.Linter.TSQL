using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines
{
    public static class UniqueRowFilterExtensions
    {
        public static bool IsDistinct(this UniqueRowFilter fltr)
         => (fltr & UniqueRowFilter.Distinct) == UniqueRowFilter.Distinct;
    }
}
