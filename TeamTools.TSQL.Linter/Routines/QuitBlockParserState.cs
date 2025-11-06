using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class QuitBlockParserState
    {
        public bool AllNextAreUnreachable { get; set; } = false;

        public int LastCheckedTokenIndex { get; set; } = -1;

        public bool NextIsUnreachable { get; set; } = false;

        public TSqlTokenType BreakCommandType { get; set; } = TSqlTokenType.None;
    }
}
