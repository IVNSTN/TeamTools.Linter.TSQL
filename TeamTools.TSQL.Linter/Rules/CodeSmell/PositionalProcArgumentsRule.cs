using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0102", "POSITIONAL_PROC_ARGS")]
    internal sealed class PositionalProcArgumentsRule : AbstractRule
    {
        private readonly SystemProcDetector systemProcDetector;

        public PositionalProcArgumentsRule() : base()
        {
            systemProcDetector = new SystemProcDetector();
        }

        public override void Visit(ExecutableProcedureReference node)
        {
            if ((node.ProcedureReference.ProcedureVariable == null)
            && systemProcDetector.IsSystemProc(node.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value))
            {
                return;
            }

            node.AcceptChildren(new ParameterVisitor(HandleNodeError));
        }

        private class ParameterVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> callback;

            public ParameterVisitor(Action<TSqlFragment, string> callback) : base()
            {
                this.callback = callback;
            }

            public override void Visit(ExecuteParameter node)
            {
                if (node.Variable is null)
                {
                    callback(node, "");
                }
            }
        }
    }
}
