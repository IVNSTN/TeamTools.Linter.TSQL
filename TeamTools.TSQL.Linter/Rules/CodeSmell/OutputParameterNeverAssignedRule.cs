using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0160", "OUTPUT_UNASSIGNED")]
    internal sealed class OutputParameterNeverAssignedRule : AbstractRule
    {
        // very similar to UnuserVariableRule
        public OutputParameterNeverAssignedRule() : base()
        {
        }

        public override void Visit(ProcedureStatementBody node)
        {
            if (node.StatementList is null)
            {
                // CLR methods
                return;
            }

            // var name, var declaration node, var assignment node
            var procOutputParams = new SortedDictionary<string, KeyValuePair<TSqlFragment, TSqlFragment>>(StringComparer.OrdinalIgnoreCase);

            foreach (var param in node.Parameters.Where(p => p.IsOutput()))
            {
                procOutputParams.Add(param.VariableName.Value, new KeyValuePair<TSqlFragment, TSqlFragment>(param.VariableName, null));
            }

            var assignments = new AssignValueVisitor(procOutputParams);
            node.AcceptChildren(assignments);

            procOutputParams.Where(p => p.Value.Value == null).ToList().ForEach(p =>
                HandleNodeError(p.Value.Key, p.Key));
        }

        private class AssignValueVisitor : TSqlFragmentVisitor
        {
            private readonly IDictionary<string, KeyValuePair<TSqlFragment, TSqlFragment>> variables;

            public AssignValueVisitor(IDictionary<string, KeyValuePair<TSqlFragment, TSqlFragment>> variables)
            {
                this.variables = variables;
            }

            public override void Visit(SelectSetVariable node) => AssignmentDetected(node.Variable.Name, node);

            public override void Visit(SetVariableStatement node) => AssignmentDetected(node.Variable.Name, node);

            public override void Visit(ExecuteParameter node)
            {
                if (node.IsOutput && node.ParameterValue is VariableReference varRef)
                {
                    AssignmentDetected(varRef.Name, node);
                }
            }

            private void AssignmentDetected(string variableName, TSqlFragment node)
            {
                if (variables.ContainsKey(variableName) && variables[variableName].Value == null)
                {
                    variables[variableName] = new KeyValuePair<TSqlFragment, TSqlFragment>(variables[variableName].Key, node);
                }
            }
        }
    }
}
