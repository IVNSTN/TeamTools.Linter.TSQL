using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0903", "SAME_VAR_MULTIPLE_OUTPUT")]
    internal sealed class MultipleOutputToSameVariableRule : AbstractRule
    {
        public MultipleOutputToSameVariableRule() : base()
        {
        }

        public override void Visit(ExecuteSpecification node)
        {
            if (!(node.ExecutableEntity is ExecutableProcedureReference proc))
            {
                return;
            }

            if (proc.Parameters?.Count == 0 && node.Variable is null)
            {
                return;
            }

            var outputVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (node.Variable != null)
            {
                outputVariables.Add(node.Variable.Name);
            }

            int n = proc.Parameters.Count;
            for (int i = 0; i < n; i++)
            {
                var param = proc.Parameters[i];
                if (param.IsOutput && param.ParameterValue is VariableReference varRef)
                {
                    if (!outputVariables.Add(varRef.Name))
                    {
                        HandleNodeError(param.ParameterValue);
                    }
                }
            }
        }
    }
}
