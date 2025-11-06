using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class ForbiddenProcCallVisitor : TSqlFragmentVisitor
    {
        private readonly string forbiddenProcPrefix = "";
        private readonly Action<TSqlFragment> callback;
        private readonly ICollection<string> forbiddenProcs = null;
        private readonly ICollection<string> exceptProcs = null;

        public ForbiddenProcCallVisitor(Action<TSqlFragment> callback, string procPrefix, ICollection<string> exceptProcList)
        {
            this.callback = callback;
            this.forbiddenProcPrefix = procPrefix;
            this.exceptProcs = new SortedSet<string>(exceptProcList, StringComparer.OrdinalIgnoreCase);
        }

        public ForbiddenProcCallVisitor(Action<TSqlFragment> callback, string procPrefix)
        {
            this.callback = callback;
            this.forbiddenProcPrefix = procPrefix;
        }

        public ForbiddenProcCallVisitor(Action<TSqlFragment> callback, ICollection<string> procList)
        {
            this.callback = callback;
            this.forbiddenProcs = new SortedSet<string>(procList, StringComparer.OrdinalIgnoreCase);
        }

        public override void Visit(ExecutableProcedureReference node)
        {
            if (node.ProcedureReference.ProcedureVariable != null)
            {
                return;
            }

            string name = node.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value;

            if (null != forbiddenProcs)
            {
                if (!forbiddenProcs.Contains(name))
                {
                    return;
                }
            }
            else if (exceptProcs != null && exceptProcs.Contains(name))
            {
                return;
            }
            else if (!name.StartsWith(forbiddenProcPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            callback(node);
        }
    }
}
