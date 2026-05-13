using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0521", "SYSPROC_RETURN_NOT_CHECKED")]
    internal sealed class SystemProcedureReturnCodeCheckRule : AbstractRule
    {
        // TODO : The list is quite long for today. Could it be reversed to avoid false-positive detections?
        // TODO : Consolidate all the metadata in resource file
        // Some of them don't return success code,
        // some are considered "write only" procs - return code can be ignored.
        private static readonly HashSet<string> IgnoredSystemProcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sp_addextendedproperty",
            "sp_updateextendedproperty",
            "sp_dropextendedproperty",
            "sp_executesql",
            "sp_bindefault",
            "sp_unbindefault",
            "sp_bindrule",
            "sp_unbindrule",

            // SQLDMO
            "sp_OADestroy",
            "sp_OAStop",
            "sp_OASetProperty",
            "sp_OAMethod",
            "sp_OAGetProperty",
            "sp_OAGetErrorInfo",

            // job management
            "sp_add_alert",
            "sp_add_category",
            "sp_add_job",
            "sp_add_jobschedule",
            "sp_add_jobserver",
            "sp_add_jobstep",
            "sp_add_notification",
            "sp_add_operator",
            "sp_add_proxy",
            "sp_add_schedule",
            "sp_add_targetservergroup",
            "sp_add_targetsvrgrp_member",

            "sp_delete_alert",
            "sp_delete_category",
            "sp_delete_job",
            "sp_delete_jobschedule",
            "sp_delete_jobserver",
            "sp_delete_jobstep",
            "sp_delete_notification",
            "sp_delete_operator",
            "sp_delete_proxy",
            "sp_delete_schedule",
            "sp_delete_targetservergroup",
            "sp_delete_targetsvrgrp_member",

            "sp_update_alert",
            "sp_update_category",
            "sp_update_job",
            "sp_update_jobschedule",
            "sp_update_jobserver",
            "sp_update_jobstep",
            "sp_update_notification",
            "sp_update_operator",
            "sp_update_proxy",
            "sp_update_schedule",
            "sp_update_targetservergroup",

            "sp_apply_job_to_targets",
            "sp_remove_job_from_targets",
            "sp_attach_schedule",
            "sp_detach_schedule",
            "sp_manage_jobs_by_login",
            "sp_notify_operator",
            "sp_start_job",
            "sp_stop_job",
            "sp_purge_jobhistory",
            "sp_readerrorlog",
            "xp_dirtree",

            "sp_column_privileges",
            "sp_columns",
            "sp_databases",
            "sp_fkeys",
            "sp_pkeys",
            "sp_server_info",
            "sp_special_columns",
            "sp_sproc_columns",
            "sp_statistics",
            "sp_stored_procedures",
            "sp_table_privileges",
            "sp_tables",

            "sp_who",
            "sp_help",
            "sp_helptext",
        };

        public SystemProcedureReturnCodeCheckRule() : base()
        {
        }

        public override void Visit(ExecuteSpecification node)
        {
            // there is var for return value
            if (null != node.Variable)
            {
                return;
            }

            if (!(node.ExecutableEntity is ExecutableProcedureReference procRef))
            {
                return;
            }

            // no check if proc is identified by variable
            if (procRef.ProcedureReference.ProcedureVariable != null)
            {
                return;
            }

            string procName = procRef.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value;

            // system proc only
            if (!SystemProcDetector.IsSystemProc(procName))
            {
                return;
            }

            // not reporting on what's ignored
            if (IgnoredSystemProcs.Contains(procName))
            {
                return;
            }

            HandleNodeError(node, procName);
        }
    }
}
