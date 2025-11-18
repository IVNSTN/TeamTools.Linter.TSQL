using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0123", "UNUSED_VARIABLE")]
    internal class UnusedVariableRule : AbstractRule
    {
        public UnusedVariableRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            DetectUnusedVariables(node, new SomeVariableVisitor());
        }

        private void DetectUnusedVariables(TSqlBatch node, SomeVariableVisitor varVisitor)
        {
            node.AcceptChildren(varVisitor);

            if (varVisitor.VariableUsage.Count == 0)
            {
                return;
            }

            foreach (var unused in varVisitor.VariableUsage)
            {
                HandleNodeError(unused.Value, unused.Key);
            }
        }

        internal class SomeVariableVisitor : TSqlFragmentVisitor
        {
            private List<VariableReference> execParams;

            // name / declaration node / first usageline
            public Dictionary<string, TSqlFragment> VariableUsage { get; } = new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

            public override void Visit(ExecuteParameter node)
            {
                (execParams ?? (execParams = new List<VariableReference>())).Add(node.Variable);
            }

            public override void Visit(ProcedureParameter node)
            {
                // say they're "used" by default
                MarkAsUsed(node.VariableName.Value);
            }

            public override void Visit(VariableReference node)
            {
                // ignoring exec params
                if (execParams != null && execParams.Contains(node))
                {
                    return;
                }

                MarkAsUsed(node.Name);
            }

            public override void Visit(DeclareVariableElement node) => RegisterVariable(node.VariableName.Value, node);

            // Note, inline-table function does not define name for output table format
            public override void Visit(DeclareTableVariableBody node) => RegisterVariable(node.VariableName?.Value, node);

            private void RegisterVariable(string name, TSqlFragment node)
            {
                if (string.IsNullOrEmpty(name))
                {
                    return;
                }

                VariableUsage.TryAdd(name, node);
            }

            private void MarkAsUsed(string varName)
            {
                if (string.IsNullOrEmpty(varName))
                {
                    return;
                }

                VariableUsage.Remove(varName);
            }
        }
    }
}
