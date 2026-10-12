using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class SystemProcedureReturnCodeCheckRule : ISqlServerMetadataConsumer
    {
        private static readonly HashSet<string> IgnorableProcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // SSDT does not check them himself in generated scripts
            "sp_addextendedproperty",
            "sp_dropextendedproperty",

            // Help procs are not supposed to be used in business-logic
            // and are supposed to return a resultset.
            "sp_help_category",
            "sp_help_fulltext_catalogs",
            "sp_help_fulltext_catalogs_cursor",
            "sp_help_fulltext_columns",
            "sp_help_fulltext_columns_cursor",
            "sp_help_fulltext_tables",
            "sp_help_fulltext_tables_cursor",
            "sp_help_job",
            "sp_help_jobactivity",
            "sp_help_jobhistory",
            "sp_help_jobschedule",
            "sp_help_operator",
            "sp_help_proxy",
            "sp_help_schedule",
            "sp_help_targetservergroup",

            // Kinda _one way_ instructions
            "sp_recompile",
            "sp_refreshsqlmodule",
            "sp_refreshview",

            // Whereas sp_xml_preparedocument result should be checked for sure it is completely unclear
            // what to do if xml document handle invalidation fails.
            "sp_xml_removedocument",
        };

        private HashSet<string> systemProcsWithReturnCode;

        public void LoadMetadata(SqlServerMetadata data)
        {
            systemProcsWithReturnCode = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var procMeta in data.Procs)
            {
                if (procMeta.Value.StatusCheck.Equals("RETURN", StringComparison.OrdinalIgnoreCase))
                {
                    systemProcsWithReturnCode.Add(procMeta.Key);
                }
            }
        }
    }
}
