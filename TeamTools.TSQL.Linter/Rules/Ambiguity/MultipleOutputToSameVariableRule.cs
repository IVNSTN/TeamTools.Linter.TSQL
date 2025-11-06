using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

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

            if (proc.Parameters?.Count == 0 && node.Variable == null)
            {
                return;
            }

            var outputVariables = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            if (node.Variable != null)
            {
                outputVariables.Add(node.Variable.Name);
            }

            foreach (var param in proc.Parameters)
            {
                if (param.IsOutput && param.ParameterValue is VariableReference varRef)
                {
                    if (!outputVariables.TryAddUnique(varRef.Name))
                    {
                        HandleNodeError(param.ParameterValue);
                    }
                }
            }
        }
    }
}
