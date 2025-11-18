using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class ForbiddenProcCallVisitor : TSqlConcreteFragmentVisitor
    {
        private readonly Action<TSqlFragment> callback;
        private readonly string forbiddenProcPrefix = "";
        private readonly HashSet<string> forbiddenProcs = null;
        private readonly HashSet<string> exceptProcs = null;

        public ForbiddenProcCallVisitor(Action<TSqlFragment> callback, string procPrefix, HashSet<string> exceptProcs)
        {
            this.callback = callback;
            this.forbiddenProcPrefix = procPrefix;
            this.exceptProcs = exceptProcs;
        }

        public ForbiddenProcCallVisitor(Action<TSqlFragment> callback, string procPrefix)
        {
            this.callback = callback;
            this.forbiddenProcPrefix = procPrefix;
        }

        public ForbiddenProcCallVisitor(Action<TSqlFragment> callback, HashSet<string> procList)
        {
            this.callback = callback;
            this.forbiddenProcs = procList;
        }

        protected Action<TSqlFragment> Callback => callback;

        public override void ExplicitVisit(ExecutableProcedureReference node)
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

            Callback(node);
        }
    }
}
