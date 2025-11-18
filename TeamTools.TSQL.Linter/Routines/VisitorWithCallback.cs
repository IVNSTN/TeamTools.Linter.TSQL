using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines
{
    public abstract class VisitorWithCallback : TSqlFragmentVisitor
    {
        protected VisitorWithCallback(Action<TSqlFragment> callback)
        {
            Callback = callback;
        }

        protected Action<TSqlFragment> Callback { get; }
    }
}
