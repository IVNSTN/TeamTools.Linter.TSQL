using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0922", "PARAM_EXPECTED_AS_OUTPUT")]
    internal sealed class BuiltInProcsArgumentOutputRule : AbstractRule
    {
        private static readonly IDictionary<string, int> SysProcOutputArgs = new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static readonly IDictionary<string, string> SysProcOutputNamedArgs = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static BuiltInProcsArgumentOutputRule()
        {
            // TODO : consolidate all the metadata about known built-in functions
            SysProcOutputArgs.Add("sp_xml_preparedocument", 0);
            SysProcOutputArgs.Add("sp_OACreate", 1);
            SysProcOutputArgs.Add("sp_OAGetProperty", 2);
            SysProcOutputArgs.Add("sp_cursor_open", 0);
            SysProcOutputArgs.Add("sp_execute", 0);
            SysProcOutputArgs.Add("sp_prepare", 0);
            SysProcOutputArgs.Add("sp_prepexec", 0);

            SysProcOutputNamedArgs.Add("sp_add_job", "@job_id");
            SysProcOutputNamedArgs.Add("sp_add_jobschedule", "@schedule_id");
            SysProcOutputNamedArgs.Add("sp_send_dbmail", "@mailitem_id");
            SysProcOutputNamedArgs.Add("sysmail_add_profile_sp", "@profile_id");
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

            if (SysProcOutputArgs.ContainsKey(procName))
            {
                VerifyParameterIsOutput(node, SysProcOutputArgs[procName]);
            }
            else if (SysProcOutputNamedArgs.ContainsKey(procName))
            {
                VerifyParameterIsOutput(node, SysProcOutputNamedArgs[procName]);
            }
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
            var matchingParam = node.Parameters
                .FirstOrDefault(p => p.Variable != null && p.Variable.Name.Equals(paramName, StringComparison.OrdinalIgnoreCase));

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
