using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public override void Visit(TSqlBatch node)
        {
            DetectUnusedVariables(node, new ScalarVariableVisitor());
            DetectUnusedVariables(node, new TableVariableVisitor());
        }

        protected static KeyValuePair<TSqlFragment, string> AggregateUnusedVars(IDictionary<string, KeyValuePair<TSqlFragment, int>> vars)
        {
            TSqlFragment firstNode = vars.FirstOrDefault(v => v.Value.Value == 0).Value.Key;
            string varList = vars
                .Where(v => v.Value.Value == 0)
                .Select(v => v.Key)
                .Aggregate(new StringBuilder(), (sb, v) => (sb.Length == 0 ? sb : sb.Append(", ")).Append(v))
                .ToString();

            return new KeyValuePair<TSqlFragment, string>(firstNode, varList);
        }

        private void DetectUnusedVariables(TSqlBatch node, SomeVariableVisitor varVisitor)
        {
            node.AcceptChildren(varVisitor);

            var scalarsUnused = AggregateUnusedVars(varVisitor.VariableUsage);

            if (!string.IsNullOrEmpty(scalarsUnused.Value))
            {
                HandleNodeError(scalarsUnused.Key, scalarsUnused.Value);
            }
        }

        protected abstract class SomeVariableVisitor : TSqlFragmentVisitor
        {
            // name / declaration node / first usageline
            private readonly IDictionary<string, KeyValuePair<TSqlFragment, int>> variableUsage =
                new Dictionary<string, KeyValuePair<TSqlFragment, int>>(StringComparer.OrdinalIgnoreCase);

            private readonly IList<VariableReference> execParams = new List<VariableReference>();

            public IDictionary<string, KeyValuePair<TSqlFragment, int>> VariableUsage => variableUsage;

            public override void Visit(ExecuteParameter node)
            {
                execParams.Add(node.Variable);
            }

            public override void Visit(ProcedureParameter node)
            {
                // say they're "used" by default
                if (VariableUsage.ContainsKey(node.VariableName.Value))
                {
                    VariableUsage[node.VariableName.Value] =
                        new KeyValuePair<TSqlFragment, int>(VariableUsage[node.VariableName.Value].Key, node.StartLine);
                }
            }

            public override void Visit(VariableReference node)
            {
                // ignoring exec params
                if (execParams.Contains(node))
                {
                    return;
                }

                if (VariableUsage.ContainsKey(node.Name))
                {
                    VariableUsage[node.Name] =
                        new KeyValuePair<TSqlFragment, int>(VariableUsage[node.Name].Key, node.StartLine);
                }
            }

            protected void RegisterVariable(string name, TSqlFragment node)
            {
                if (string.IsNullOrEmpty(name))
                {
                    return;
                }

                VariableUsage.TryAdd(name, new KeyValuePair<TSqlFragment, int>(node, 0));
            }
        }

        protected class ScalarVariableVisitor : SomeVariableVisitor
        {
            public override void Visit(DeclareVariableElement node) => RegisterVariable(node.VariableName.Value, node);
        }

        protected class TableVariableVisitor : SomeVariableVisitor
        {
            // inline-table function does not define name for output table format
            public override void Visit(DeclareTableVariableBody node) => RegisterVariable(node.VariableName?.Value, node);
        }
    }
}
