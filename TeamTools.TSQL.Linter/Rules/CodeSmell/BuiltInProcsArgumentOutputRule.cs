using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0922", "PARAM_EXPECTED_AS_OUTPUT")]
    internal sealed class BuiltInProcsArgumentOutputRule : AbstractRule
    {
        private static readonly Dictionary<string, int> SysProcOutputArgs;
        private static readonly Dictionary<string, string> SysProcOutputNamedArgs;

        static BuiltInProcsArgumentOutputRule()
        {
            // TODO : consolidate all the metadata about known built-in functions
            SysProcOutputArgs = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "sp_xml_preparedocument", 0 },
                { "sp_OACreate", 1 },
                { "sp_OAGetProperty", 2 },
                { "sp_cursor_open", 0 },
                { "sp_execute", 0 },
                { "sp_prepare", 0 },
                { "sp_prepexec", 0 },
            };

            SysProcOutputNamedArgs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "sp_add_job", "@job_id" },
                { "sp_add_jobschedule", "@schedule_id" },
                { "sp_send_dbmail", "@mailitem_id" },
                { "sysmail_add_profile_sp", "@profile_id" },
            };
        }

        public BuiltInProcsArgumentOutputRule() : base()
        {
        }

        public override void Visit(ExecutableProcedureReference node)
        {
            if (node.ProcedureReference.ProcedureReference is null)
            {
                // exec @proc_name
                return;
            }

            if (node.Parameters.Count == 0)
            {
                return;
            }

            var procName = node.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value;

            if (SysProcOutputArgs.TryGetValue(procName, out var posArg))
            {
                VerifyParameterIsOutput(node, posArg);
            }
            else if (SysProcOutputNamedArgs.TryGetValue(procName, out var namedArg))
            {
                VerifyParameterIsOutput(node, namedArg);
            }
        }

        private static ExecuteParameter MatchParamByName(string paramName, IList<ExecuteParameter> parameters)
        {
            for (int i = parameters.Count - 1; i >= 0; i--)
            {
                var p = parameters[i];
                if (p.Variable != null
                && p.Variable.Name.Equals(paramName, StringComparison.OrdinalIgnoreCase))
                {
                    return p;
                }
            }

            return default;
        }

        private void VerifyParameterIsOutput(ExecutableProcedureReference node, int paramIndex)
        {
            if (paramIndex < node.Parameters.Count)
            {
                VerifyParameterIsOutput(node.Parameters[paramIndex]);
            }
        }

        private void VerifyParameterIsOutput(ExecutableProcedureReference node, string paramName)
        {
            var matchingParam = MatchParamByName(paramName, node.Parameters);

            if (matchingParam != null)
            {
                VerifyParameterIsOutput(matchingParam);
            }
        }

        private void VerifyParameterIsOutput(ExecuteParameter node)
        {
            if (!node.IsOutput)
            {
                HandleNodeError(node, node.Variable?.Name);
            }
        }
    }
}
