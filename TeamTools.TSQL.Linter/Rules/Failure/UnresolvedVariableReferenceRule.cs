using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0458", "UNRESOLVED_VARIABLED_NAME")]
    internal sealed class UnresolvedVariableReferenceRule : AbstractRule
    {
        public UnresolvedVariableReferenceRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node) => node.AcceptChildren(new VarVisitor(ViolationHandlerWithMessage));

        private sealed class VarVisitor : TSqlFragmentVisitor
        {
            private readonly HashSet<string> variables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            private readonly Action<TSqlFragment, string> callback;

            public VarVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            // This will also visit input parameter declarations
            public override void Visit(DeclareVariableElement node)
            {
                variables.Add(node.VariableName.Value);
            }

            public override void ExplicitVisit(ExecuteParameter node)
            {
                // parameter value can be variable reference
                node.ParameterValue?.Accept(this);
            }

            // Ignoring table variable references - they are validated by separate rule
            public override void ExplicitVisit(VariableTableReference node)
            { }

            public override void Visit(VariableReference node)
            {
                if (!variables.Contains(node.Name))
                {
                    callback(node, node.Name);
                }
            }
        }
    }
}
