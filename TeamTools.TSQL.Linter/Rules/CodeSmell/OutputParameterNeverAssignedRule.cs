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
        // very similar to UnusedVariableRule
        public OutputParameterNeverAssignedRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                DoValidate(proc);
            }
        }

        private static IEnumerable<ProcedureParameter> GetOutputParams(IList<ProcedureParameter> parameters)
        {
            for (int i = parameters.Count - 1; i >= 0; i--)
            {
                var p = parameters[i];
                if (p.IsOutput())
                {
                    yield return p;
                }
            }
        }

        private void DoValidate(ProcedureStatementBody node)
        {
            if (node.StatementList is null)
            {
                // CLR methods
                return;
            }

            if (node.Parameters.Count == 0)
            {
                // no params to watch for
                return;
            }

            var outputParams = GetOutputParams(node.Parameters);

            if (!outputParams.Any())
            {
                return;
            }

            var unassignedParams = outputParams.ToDictionary(p => p.VariableName.Value, p => (TSqlFragment)p, StringComparer.OrdinalIgnoreCase);
            var assignments = new AssignValueVisitor(unassignedParams);
            node.AcceptChildren(assignments);

            // All assigned params were removed from the dictionary by assignments visitor
            foreach (var unassignedOutputParam in unassignedParams)
            {
                if (unassignedOutputParam.Value != null)
                {
                    HandleNodeError(unassignedOutputParam.Value, unassignedOutputParam.Key);
                }
            }
        }

        private sealed class AssignValueVisitor : TSqlFragmentVisitor
        {
            private readonly Dictionary<string, TSqlFragment> variables;

            public AssignValueVisitor(Dictionary<string, TSqlFragment> variables)
            {
                this.variables = variables;
            }

            public override void Visit(SelectSetVariable node) => AssignmentDetected(node.Variable.Name);

            public override void Visit(SetVariableStatement node) => AssignmentDetected(node.Variable.Name);

            public override void Visit(ExecuteParameter node)
            {
                if (node.IsOutput && node.ParameterValue is VariableReference varRef)
                {
                    AssignmentDetected(varRef.Name);
                }
            }

            private void AssignmentDetected(string variableName) => variables.Remove(variableName);
        }
    }
}
